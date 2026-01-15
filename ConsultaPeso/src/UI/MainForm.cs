using ConsultaPeso.Application;
using ConsultaPeso.Domain;
using ConsultaPeso.Enum;
using ConsultaPeso.Readers;

namespace ConsultaPeso;

public partial class MainForm : Form
{
    private readonly ConsultaPesosService _service;
    private CancellationTokenSource? _cts;

    public MainForm(ConsultaPesosService service)
    {
        _service = service;
        //-- Integración con lector de tarjetas --
        // Usar BarcodeKeyboardReader para lectores que emulan teclado con código de barras
        // Puede cambiarse por QrKeyboardReader si se usa un lector de QR o KeyboardCardReader para lectores genéricos
        _cardReader = new BarcodeKeyboardReader();
        _cardReader.CardRead += OnCardRead;


        Text = "Consulta de Pesos";
        Width = 800;
        Height = 520;
        StartPosition = FormStartPosition.CenterScreen;


        InicializarLayout();
        InicializarHeader();
        InicializarResumenSemana();
        InicializarDetalleDia();
        
        //ConfigurarModoKiosco();

    }

    private void ConfigurarModoKiosco()
    {
        FormBorderStyle = FormBorderStyle.None;
        WindowState = FormWindowState.Maximized;
        TopMost = true;

        // Evita que aparezca en la barra de tareas
        ShowInTaskbar = false;
        BloquearMouseDerecho(this);

    }

    private bool _permitirCerrar = false;

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        // Solo permitir cierre programado
        if (!_permitirCerrar)
        {
            e.Cancel = true;
            return;
        }

        base.OnFormClosing(e);
    }
    protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
    {
        if (keyData == (Keys.Control | Keys.Shift | Keys.Q))
        {
            _permitirCerrar = true;
            Close();
            return true;
        }

        return base.ProcessCmdKey(ref msg, keyData);
    }

    private void BloquearMouseDerecho(Control control)
    {
        control.MouseDown += (s, e) =>
        {
            if (e.Button == MouseButtons.Right)
                return;
        };

        foreach (Control child in control.Controls)
            BloquearMouseDerecho(child);
    }


    #region Layout

    private TableLayoutPanel mainLayout;
    private TableLayoutPanel headerLayout;
    private SplitContainer splitContainer;

    private void InicializarLayout()
    {
        mainLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            RowCount = 2
        };
        mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 130));
        mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 700));

        headerLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            RowCount = 2
        };
        headerLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 45));
        headerLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));

        splitContainer = new SplitContainer
        {
            Dock = DockStyle.Fill,
            Orientation = Orientation.Vertical,
            FixedPanel = FixedPanel.None,
            IsSplitterFixed = true,
            SplitterWidth = 5,
        };

        mainLayout.Controls.Add(headerLayout, 0, 0);
        mainLayout.Controls.Add(splitContainer, 0, 1);

        Controls.Add(mainLayout);
    }

    #endregion

    #region Header

    private FlowLayoutPanel namePanel;

    private TextBox txtCodigoEmpleado;
    private Button btnBuscar;
    private TextBox txtNombreEmpleado;
    private Button btnBorrar;

    private GroupBox grpOpciones;
    private CheckBox btnSemanaActual;
    private CheckBox btnSemanaMenos1;
    private CheckBox btnSemanaMenos2;

    private void InicializarHeader()
    {
        namePanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(10, 0, 10, 0),
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false
        };


        txtCodigoEmpleado = new TextBox
        {
            PlaceholderText = "Código",
            ReadOnly = false,
            Width = 100,
            Font = new Font(FontFamily.GenericSansSerif, 15),
        };
        txtCodigoEmpleado.Enter += (_, _) => txtCodigoEmpleado.SelectAll();
        
        //-- Integración con lector de tarjetas --
        txtCodigoEmpleado.KeyPress += (s, e) => _cardReader.ProcessKey(e.KeyChar);
        
        txtNombreEmpleado = new TextBox
        {
            Width = 300,
            ReadOnly = true,
            Font = new Font(FontFamily.GenericSansSerif, 15),
        };

        btnBuscar = new Button { Text = "⌕", Width = 35, Height = 35 };
        btnBorrar = new Button { Text = "🗑", Width = 35, Height = 35 };
        this.AcceptButton = btnBuscar;
        btnBuscar.Click += (s, e) =>
        {
            SeleccionarPeriodo(PeriodoSemana.Actual);
            BuscarEmpleado(s, e);
        };

        btnBorrar.Click += BorrarDatos;


        namePanel.Controls.AddRange(new Control[]
        {
            txtCodigoEmpleado,
            btnBuscar,
            txtNombreEmpleado,
            btnBorrar
        });

        headerLayout.Controls.Add(namePanel, 0, 0);

        CrearGrupoHorizontal();
    }

    

    #endregion

    #region GrupoOpciones
    private RadioButton rbSemanaActual;
    private RadioButton rbSemanaAnterior;
    private RadioButton rbSemanaAnteAnterior;

    public void CrearGrupoHorizontal()
    {
        var grpOpciones = new GroupBox
        {
            Text = "Opciones",
            AutoSize = true,
            Padding = new Padding(10),
            Location = new Point(20, 20)
        };

        var panel = new FlowLayoutPanel
        {
            FlowDirection = FlowDirection.LeftToRight,
            AutoSize = true,
            WrapContents = false, // true si quieres que salte a la siguiente línea
            Dock = DockStyle.Fill
        };

        rbSemanaAnteAnterior = CrearRadioButton("Semana -2", PeriodoSemana.AnteAnterior);
        rbSemanaAnterior     = CrearRadioButton("Semana -1", PeriodoSemana.Anterior);
        rbSemanaActual       = CrearRadioButton("Semana Actual", PeriodoSemana.Actual, true);

        panel.Controls.Add(rbSemanaAnteAnterior);
        panel.Controls.Add(rbSemanaAnterior);
        panel.Controls.Add(rbSemanaActual);

        grpOpciones.Controls.Add(panel);
        headerLayout.Controls.Add(grpOpciones, 0, 1);
    }

    private RadioButton CrearRadioButton(string texto, PeriodoSemana periodo, bool seleccionado = false)
    {
        var rb = new RadioButton
        {
            Text = texto,
            Checked = seleccionado,
            AutoSize = true,
            Tag = periodo
        };

        rb.CheckedChanged += PeriodoCambiado;

        return rb;
    }
    private void PeriodoCambiado(object? sender, EventArgs e)
    {
        if (sender is not RadioButton rb || !rb.Checked)
            return;

        SeleccionarPeriodo((PeriodoSemana)rb.Tag);
        BuscarEmpleado(sender, e);
    }

    private void SeleccionarPeriodo(PeriodoSemana periodo)
    {
        _periodoSeleccionado = periodo;

        rbSemanaActual.Checked       = periodo == PeriodoSemana.Actual;
        rbSemanaAnterior.Checked     = periodo == PeriodoSemana.Anterior;
        rbSemanaAnteAnterior.Checked = periodo == PeriodoSemana.AnteAnterior;
    }

    #endregion

    #region ResumenSemanal

    private ListView lvResumenSemana;

    private void InicializarResumenSemana()
    {
        lvResumenSemana = new ListView
        {
            Dock = DockStyle.Fill,
            View = View.Details,
            FullRowSelect = true
        };

        lvResumenSemana.Columns.Add("Día", 80);
        lvResumenSemana.Columns.Add("Libra", 80);
        lvResumenSemana.Columns.Add("Valor", 80);

        lvResumenSemana.SelectedIndexChanged += DiaSeleccionado;

        splitContainer.Panel1.Controls.Add(lvResumenSemana);
    }

    #endregion

    #region DetallesDia

    private TableLayoutPanel panelTotales;
    private ListView lvDetalleDia;
    private Label lblPesosValor;
    private Label lblLbValor;
    private Label lblValorValor;

    private void InicializarDetalleDia()
    {
        lvDetalleDia = new ListView
        {
            Dock = DockStyle.Fill,
            View = View.Details,
            FullRowSelect = true
        };

        panelTotales = new TableLayoutPanel
        {
            Dock = DockStyle.Bottom,
            ColumnCount = 6,
            Height = 40
        };


        lvDetalleDia.Columns.Add("Descripcion", 80);
        lvDetalleDia.Columns.Add("Hora", 80);
        lvDetalleDia.Columns.Add("Libra", 80);
        lvDetalleDia.Columns.Add("Valor", 80);

        int separator = 60;
        panelTotales.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, separator));
        panelTotales.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, separator));
        panelTotales.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, separator));
        panelTotales.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, separator));
        panelTotales.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, separator));
        panelTotales.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, separator));
        
        var lblPesosTitulo = CrearLabelTotal("Pesos");
        var lblLbTitulo = CrearLabelTotal("Libra");
        var lblValorTitulo = CrearLabelTotal("Valor");
        
        lblPesosValor = CrearValorTotal();
        lblLbValor = CrearValorTotal();
        lblValorValor = CrearValorTotal();

        panelTotales.Controls.Add(lblPesosTitulo, 0,0);
        panelTotales.Controls.Add(lblLbTitulo, 2,0);
        panelTotales.Controls.Add(lblValorTitulo, 4,0);
        
        panelTotales.Controls.Add(lblPesosValor, 1,0);
        panelTotales.Controls.Add(lblLbValor, 3,0);
        panelTotales.Controls.Add(lblValorValor, 5,0);

        splitContainer.Panel2.Controls.Add(lvDetalleDia);
        splitContainer.Panel2.Controls.Add(panelTotales);
    }
    private Label CrearLabelTotal(string titulo)
    {
        return new Label
        {
            Text = @$"{titulo}:",
            AutoSize = false,
            TextAlign = ContentAlignment.MiddleLeft,
            Font = new Font("Segoe UI", 10, FontStyle.Regular)
        };
    }
    private Label CrearValorTotal()
    {
        return new Label
        {
            AutoSize = false,
            TextAlign = ContentAlignment.MiddleLeft,
            Font = new Font("Segoe UI", 12, FontStyle.Bold)

        };
    }
    #endregion

    protected override void OnShown(EventArgs e)
    {
        base.OnShown(e);

        splitContainer.SplitterDistance = splitContainer.Width / 2;
    }

    private PeriodoSemana _periodoSeleccionado = PeriodoSemana.Actual;
    private ConsultaPesosRequest? _currentConsulta;

    private void BorrarDatos(object? sender, EventArgs e)
    {
        txtCodigoEmpleado.Clear();
        txtNombreEmpleado.Clear();
        lvResumenSemana.Items.Clear();
        lvDetalleDia.Items.Clear();
        _currentConsulta = null;
        lblLbValor.Text = "";
        lblPesosValor.Text = "";
        lblValorValor.Text = "";
    }
    private async void BuscarEmpleado(object? sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(txtCodigoEmpleado.Text))
        {
            return;
        }
        
        btnBuscar.Enabled = false;

        _cts?.Cancel();
        _cts = new CancellationTokenSource();

        try
        {
            var fecha = DateOnly.FromDateTime(DateTime.Today).AddDays((int)_periodoSeleccionado);

            _currentConsulta = await _service.ConsultarSemanaAsync(
                txtCodigoEmpleado.Text.Trim(),
                fecha,
                _cts.Token);

            CargarResumenSemanal();
        }
        catch (OperationCanceledException)
        {
            // cancelación normal
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Error",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            btnBuscar.Enabled = true;
        
            txtCodigoEmpleado.Focus();
            txtCodigoEmpleado.SelectAll();
        }

    }
    private void DiaSeleccionado(object? sender, EventArgs e)
    {
        if (_currentConsulta == null)
            return;

        if (lvResumenSemana.SelectedItems.Count == 0)
            return;

        var selectedItem = lvResumenSemana.SelectedItems[0];

        // Ignorar fila TOTAL
        if (selectedItem.Text == "TOTAL")
            return;

        if (selectedItem.Tag is not DateOnly fechaSeleccionada)
            return;

        lvDetalleDia.BeginUpdate();
        lvDetalleDia.Items.Clear();

        decimal totalLibras = 0;
        decimal totalValor = 0;
        int cantidadPesos = 0;

        var pesosDelDia = _currentConsulta!.Pesos
            .Where(p => DateOnly.FromDateTime(p.Fecha) == fechaSeleccionada);

        foreach (var peso in pesosDelDia)
        {
            var valor = peso.Valor * peso.Precio;

            totalLibras += peso.Valor;
            totalValor += valor;
            cantidadPesos++;

            var item = new ListViewItem(peso.Descripcion);
            item.SubItems.Add(peso.Fecha.ToString("HH:mm"));
            item.SubItems.Add(peso.Valor.ToString("N2"));
            item.SubItems.Add(valor.ToString("N2"));

            lvDetalleDia.Items.Add(item);
        }

        lvDetalleDia.EndUpdate();

        
        // Mostrar totales del día
        lblPesosValor.Text = cantidadPesos.ToString("N0");
        lblLbValor.Text = totalLibras.ToString("N2");
        lblValorValor.Text = @$"{totalValor.ToString("N2")} $";
        
    }
    
    private DateOnly ObtenerFechaSeleccionInicial()
    {
        if (_periodoSeleccionado == PeriodoSemana.Actual)
            return DateOnly.FromDateTime(DateTime.Today);

        return _currentConsulta!.FechaInicio; // lunes
    }


    private void CargarResumenSemanal()
    {
        if (_currentConsulta == null)
            return;

        lvResumenSemana.BeginUpdate();
        lvResumenSemana.Items.Clear();

        decimal totalSemanaLibra = 0;
        decimal totalSemanaValor = 0;

        // ✅ Agrupar correctamente por día
        var pesosPorDia = _currentConsulta.Pesos
            .GroupBy(p => DateOnly.FromDateTime(p.Fecha))
            .ToDictionary(g => g.Key, g => g.ToList());

        var fecha = _currentConsulta.FechaInicio; // DateOnly (lunes)

        for (int i = 0; i < 7; i++)
        {
            decimal librasDia = 0;
            decimal valorDia = 0;

            if (pesosPorDia.TryGetValue(fecha, out var pesosDia))
            {
                librasDia = pesosDia.Sum(p => p.Valor);
                valorDia = pesosDia.Sum(p => p.Valor * p.Precio);
            }

            totalSemanaLibra += librasDia;
            totalSemanaValor += valorDia;

            var item = new ListViewItem($"{fecha:dddd dd}")
            {
                Tag = fecha // 🔥 sigue siendo DateOnly (bien)
            };

            item.SubItems.Add(librasDia.ToString("N2"));
            item.SubItems.Add(valorDia.ToString("N2"));

            lvResumenSemana.Items.Add(item);

            fecha = fecha.AddDays(1);
        }

        // 🔹 FILA TOTAL
        var totalItem = new ListViewItem("TOTAL")
        {
            Font = new Font(lvResumenSemana.Font, FontStyle.Bold),
            BackColor = Color.Gainsboro
        };

        totalItem.SubItems.Add(totalSemanaLibra.ToString("N2"));
        totalItem.SubItems.Add(totalSemanaValor.ToString("N2"));

        lvResumenSemana.Items.Add(totalItem);

        lvResumenSemana.EndUpdate();
        txtNombreEmpleado.Text = _currentConsulta.Empleado.NombreCompleto;
        
        var fechaSeleccion = ObtenerFechaSeleccionInicial();

        foreach (ListViewItem item in lvResumenSemana.Items)
        {
            if (item.Tag is DateOnly fechar && fechar == fechaSeleccion)
            {
                item.Selected = true;
                item.Focused = true;
                item.EnsureVisible();
                break;
            }
        }

    }


    #region Card Reader
    
    private readonly ICardReader _cardReader;
    
    private void OnCardRead(object? sender, string codigo)
    {
        if (!btnBuscar.Enabled)
            return;

        if (InvokeRequired)
        {
            BeginInvoke(() => OnCardRead(sender, codigo));
            return;
        }

        txtCodigoEmpleado.Text = codigo;
        btnBuscar.PerformClick();
    }


    #endregion
}
