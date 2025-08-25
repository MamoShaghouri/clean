using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using MouseButtonState = System.Windows.Input.MouseButtonState;

using Shaghouri;
using Microsoft.Win32;
using System.IO;
using WpfPoint = System.Windows.Point;
using WpfKeyEventArgs = System.Windows.Input.KeyEventArgs;
using System.Text.Json;
using System.Globalization;

// Services
using Shaghouri.Services;
using Shaghouri.Models;

namespace Shaghouri
{
    /// <summary>
    /// Interaction logic for Create.xaml
    /// </summary>
    public partial class Create : Window
    {
        #region Services
        
        private DrawingService _drawingService = null!;
        private CanvasService _canvasService = null!;
        private GrainLineService _grainLineService = null!;
        private FileService _fileService = null!;
        
        #endregion
        
        #region State
        
        private DrawingState _drawingState = null!;
        private AppSettings settings = null!;
        
        #endregion
        
        #region UI State
        
        public List<RectangleShape> rectangles = new List<RectangleShape>();
        private bool IsShapeFillEnabled = true;
        private bool IsLineSizeEnabled = true;
        private bool IsAnchorPointsEnabled = true;
        private System.Windows.Controls.ContextMenu leftCanvasContextMenu = null!;
        
        #endregion
        
        #region Constructor and Initialization

        public Create()
        {
            InitializeComponent();
            InitializeServices();
            InitializeState();
            InitializeUI();
            SetupEventHandlers();
            SetupLeftCanvasContextMenu();
        }
        
        private void InitializeServices()
        {
            _drawingService = new DrawingService();
            _canvasService = new CanvasService();
            _grainLineService = new GrainLineService();
            _fileService = new FileService();
        }
        
        private void InitializeState()
        {
            _drawingState = new DrawingState();
            settings = AppSettings.Load();
        }
        
        private void InitializeUI()
        {
            this.DataContext = this;
            cm.IsChecked = true;
            SelectedUnit = UnitService.UnitType.CM;
            
            // الوضع الافتراضي: عرض اللوحتين معًا وبنفس العرض
            _canvasService.InitializeDefaultCanvasLayout(canvasGrid, LeftCanvas, RightCanvas);
            
            ApplySettingsToUI();
            
            // تهيئة DPI للوحدات
            UnitService.UpdateDpiScaleFromWindow(this);
            
            this.Closing += (s, e) => SaveSettingsFromUI();
        }
        
        private void SetupEventHandlers()
        {
            // Canvas buttons
            btnLeftCanvasTop.Click += btnLeftCanvasTop_Click;
            btnRightCanvasTop.Click += btnRightCanvasTop_Click;
            btnBothCanvasTop.Click += btnBothCanvasTop_Click;
            btnLeftTop.Click += btnLeftTop_Click;
            
            // Drawing buttons
            btnRectangle.Click += btnRectangle_Click;
            btnEsc.Click += btnEsc_Click;
            btnFill.Click += btnFill_Click;
            btnToggleSize.Click += btnToggleSize_Click;
            btnPointsShow_hide.Click += btnPointsShow_hide_Click;
            btnSizes.Click += btnSizes_Click;
            
            // Other buttons
            btnSe1.Click += btnSe1_Click;
            btnRecycleBin.Click += btnRecycleBin_Click;
            btnRightShow.Click += btnRightShow_Click;
            
            // Grid color buttons
            btnLeftCanvasChangeGridColor.Click += btnLeftCanvasChangeGridColor_Click;
            btnRighttCanvasChangeGridColor.Click += btnRighttCanvasChangeGridColor_Click;
            
            // Grid spacing textboxes
            TextBoxLeftCanvasGridSpacingWidth.TextChanged += TextBoxLeftCanvasGridSpacingWidth_TextChanged;
            TextBoxLeftCanvasGridSpacingHeight.TextChanged += TextBoxLeftCanvasGridSpacingHeight_TextChanged;
            TextBoxRightCanvasGridSpacingWidth.TextChanged += TextBoxRightCanvasGridSpacingWidth_TextChanged;
            TextBoxRightCanvasGridSpacingHeight.TextChanged += TextBoxRightCanvasGridSpacingHeight_TextChanged;
            
            // Background buttons
            btnLeftCanvasToggleBackground.Click += btnLeftCanvasToggleBackground_Click;
            btnLeftCanvasChangeBackgroundColor.Click += btnLeftCanvasChangeBackgroundColor_Click;
            btnRightCanvasToggleBackground.Click += btnRightCanvasToggleBackground_Click;
            btnRightCanvasChangeBackgroundColor.Click += btnRightCanvasChangeBackgroundColor_Click;
            
            // Grid show/hide buttons
            btnLeftCanvasShow_HideGrid.Click += btnLeftCanvasShow_HideGrid_Click;
            btnRighttCanvasShow_HideGrid.Click += btnRighttCanvasShow_HideGrid_Click;
            
            // Sliders
            SilderLeftCanvasGridOpacity.ValueChanged += SilderLeftCanvasGridOpacity_ValueChanged;
            SilderRightCanvasGridOpacity.ValueChanged += SilderRightCanvasGridOpacity_ValueChanged;
            SilderLeftCanvasbackgroundOpacity.ValueChanged += SilderLeftCanvasbackgroundOpacity_ValueChanged;
            SilderRightCanvasbackgroundOpacity.ValueChanged += SilderRightCanvasbackgroundOpacity_ValueChanged;
            
            // Canvas events
            LeftCanvas.MouseLeftButtonDown += LeftCanvas_MouseLeftButtonDown;
            LeftCanvas.MouseMove += LeftCanvas_MouseMove;
            LeftCanvas.MouseLeftButtonUp += LeftCanvas_MouseLeftButtonUp;
            LeftCanvas.PreviewMouseRightButtonDown += LeftCanvas_PreviewMouseRightButtonDown;
        }
        
        #endregion
        
        #region Unit Management

        // استخدام الخدمة المركزية للوحدات
        public UnitService.UnitType SelectedUnit 
        { 
            get => UnitService.CurrentUnit;
            set => UnitService.CurrentUnit = value;
        }
        
        #endregion
        
        #region Shape Preview
        
        private Shape _selectedShape = null!;
        public Shape selectedShape
        {
            get => _selectedShape;
            set
            {
                _selectedShape = value;
                System.Diagnostics.Debug.WriteLine($"[Create] selectedShape property set to: {_selectedShape}");
            }
        }

        public List<Shape> SelectedShapes
        {
            get
            {
                if (LeftCanvas != null)
                    return LeftCanvas.GetSelectedShapes() ?? new List<Shape>();
                return new List<Shape>();
            }
        }
        
        public void TabControl_SelectionChanged_3(object sender, SelectionChangedEventArgs e)
        {
            // يمكنك إضافة منطق تغيير التبويب هنا
        }
        
        #endregion
    }
} 