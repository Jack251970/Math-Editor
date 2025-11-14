using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace Editor;

public partial class EquationToolBar : UserControl
{
    public event EventHandler? CommandCompleted = null;
    private readonly Dictionary<object, ButtonPanel> buttonPanelMapping = [];
    private ButtonPanel? visiblePanel = null;
    private readonly IMainWindow _mainWindow = null!;

    public EquationToolBar() : this(null!)
    {
    }

    public EquationToolBar(IMainWindow mainWindow)
    {
        _mainWindow = mainWindow;
        InitializeComponent();
    }

    private void ToolBarButton_Click(object sender, RoutedEventArgs e)
    {
        TryHideVisiblePanel();
        SetActivePanel(sender);
    }

    private void ToolBarButton_PointerEntered(object? sender, PointerEventArgs e)
    {
        if (sender is Button)
        {
            ChangeActivePanel(sender);
        }
    }

    private void ToolBarButton_GotFocus(object? sender, GotFocusEventArgs e)
    {
        if (sender is Button)
        {
            ChangeActivePanel(sender);
        }
    }

    private void ChangeActivePanel(object sender)
    {
        if (TryHideVisiblePanel())
        {
            SetActivePanel(sender);
        }
    }

    public bool TryHideVisiblePanel()
    {
        if (visiblePanel != null)
        {
            visiblePanel.IsVisible = false;
            visiblePanel = null;
            return true;
        }
        return false;
    }

    private void SetActivePanel(object sender)
    {
        if (Design.IsDesignMode) return;
        buttonPanelMapping[sender].IsVisible = true;
        visiblePanel = buttonPanelMapping[sender];
        _mainWindow.TryHideCharacterToolBarVisiblePanel();
    }

    private void UserControl_Loaded(object sender, RoutedEventArgs e)
    {
        if (Design.IsDesignMode) return;
        CreateBracketsPanel();
        CreateSumsProductsPanel();
        CreateIntegralsPanel();
        CreateSubAndSuperPanel();
        CreateDivAndRootsPanel();
        CreateCompositePanel();
        CreateDecoratedEquationPanel();
        CreateDecoratedCharacterPanel();
        CreateArrowEquationPanel();
        CreateBoxEquationPanel();
        CreateMatrixPanel();
    }

    private void CreatePanel(List<CommandDetails> list, Button toolBarButton, int columns, int padding)
    {
        var bp = new ButtonPanel(_mainWindow, list, columns, padding);
        bp.ButtonClick += (x, y) => { CommandCompleted?.Invoke(this, EventArgs.Empty); visiblePanel = null; };
        mainToolBarPanel.Children.Add(bp);
        Canvas.SetTop(bp, mainToolBarPanel.Height);
        var offset = this.GetOffset(toolBarButton);
        Canvas.SetLeft(bp, offset.X + 2);
        bp.IsVisible = false;
        buttonPanelMapping.Add(toolBarButton, bp);
    }

    private void CreateImagePanel(string[] imageUris, CommandType[] commands, object[] paramz, Button toolBarButton, int columns)
    {
        var items = new Image[imageUris.Length];
        for (var i = 0; i < items.Length; i++)
        {
            items[i] = new Image();
            var bmi = ImageHelper.GetBitmap(imageUris[i]);
            items[i].Source = bmi;
        }
        List<CommandDetails> list = [];
        for (var i = 0; i < items.Length; i++)
        {
            list.Add(new CommandDetails { Image = items[i], CommandType = commands[i], CommandParam = paramz[i] });
        }
        CreatePanel(list, toolBarButton, columns, 4);
    }

    private static string CreateImagePath(string subFolder, string imageFileName)
    {
        var sb = new StringBuilder();
        sb.Append("avares://Editor/Images/Commands/").Append(subFolder).Append('/').Append(imageFileName);
        return sb.ToString();
    }

    private void CreateBracketsPanel()
    {
        string[] imageUris = [
            CreateImagePath("Brackets", "Parentheses.png"),
            CreateImagePath("Brackets", "SquareBracket.png"),
            CreateImagePath("Brackets", "CurlyBracket.png"),
            CreateImagePath("Brackets", "PointingAngles.png"),

            CreateImagePath("Brackets", "SingleBar.png"),
            CreateImagePath("Brackets", "DoubleBar.png"),
            CreateImagePath("Brackets", "Floor.png"),
            CreateImagePath("Brackets", "Ceiling.png"),

            CreateImagePath("Brackets", "SquareParenthesis.png"),
            CreateImagePath("Brackets", "ParenthesisSquare.png"),
            CreateImagePath("Brackets", "BarAngle.png"),
            CreateImagePath("Brackets", "AngleBar.png"),

            CreateImagePath("Brackets", "LeftLeftSquareBracket.png"),
            CreateImagePath("Brackets", "RightRightSquareBracket.png"),
            CreateImagePath("Brackets", "RightLeftSquareBracket.png"),
            CreateImagePath("Brackets", "SquareBar.png"),

            CreateImagePath("Brackets", "LeftParenthesis.png"),
            CreateImagePath("Brackets", "RightParenthesis.png"),
            CreateImagePath("Brackets", "LeftSquareBracket.png"),
            CreateImagePath("Brackets", "RightSquareBracket.png"),

            CreateImagePath("Brackets", "LeftCurlyBracket.png"),
            CreateImagePath("Brackets", "RightCurlyBracket.png"),
            CreateImagePath("Brackets", "LeftAngle.png"),
            CreateImagePath("Brackets", "RightAngle.png"),

            CreateImagePath("Brackets", "LeftBar.png"),
            CreateImagePath("Brackets", "RightBar.png"),
            CreateImagePath("Brackets", "LeftDoubleBar.png"),
            CreateImagePath("Brackets", "RightDoubleBar.png"),

            CreateImagePath("Brackets", "LeftSquareBar.png"),
            CreateImagePath("Brackets", "RightSquareBar.png"),
            CreateImagePath("Brackets", "DoubleArrowBarBracket.png"),
            CreateImagePath("Brackets", "DoubleArrowBarBracket.png"),  //empty cell

            CreateImagePath("Brackets", "TopCurlyBracket.png"),
            CreateImagePath("Brackets", "BottomCurlyBracket.png"),
            CreateImagePath("Brackets", "TopSquareBracket.png"),
            CreateImagePath("Brackets", "BottomSquareBracket.png"),
        ];
        CommandType[] commands = [
            CommandType.LeftRightBracket, CommandType.LeftRightBracket,
            CommandType.LeftRightBracket, CommandType.LeftRightBracket,

            CommandType.LeftRightBracket, CommandType.LeftRightBracket,
            CommandType.LeftRightBracket, CommandType.LeftRightBracket,

            CommandType.LeftRightBracket, CommandType.LeftRightBracket,
            CommandType.LeftRightBracket, CommandType.LeftRightBracket,

            CommandType.LeftRightBracket, CommandType.LeftRightBracket,
            CommandType.LeftRightBracket, CommandType.LeftRightBracket,

            CommandType.LeftBracket,      CommandType.RightBracket,
            CommandType.LeftBracket,      CommandType.RightBracket,

            CommandType.LeftBracket,      CommandType.RightBracket,
            CommandType.LeftBracket,      CommandType.RightBracket,

            CommandType.LeftBracket,      CommandType.RightBracket,
            CommandType.LeftBracket,      CommandType.RightBracket,

            CommandType.LeftBracket,      CommandType.RightBracket,
            CommandType.DoubleArrowBarBracket, CommandType.None,  //empty cell

            CommandType.TopBracket,  CommandType.BottomBracket,
            CommandType.TopBracket, CommandType.BottomBracket,
            ];
        object[] paramz = [
            new BracketSignType [] {BracketSignType.LeftRound,     BracketSignType.RightRound},
            new BracketSignType [] {BracketSignType.LeftSquare,    BracketSignType.RightSquare},
            new BracketSignType [] {BracketSignType.LeftCurly,     BracketSignType.RightCurly},
            new BracketSignType [] {BracketSignType.LeftAngle,     BracketSignType.RightAngle},

            new BracketSignType [] {BracketSignType.LeftBar,       BracketSignType.RightBar},
            new BracketSignType [] {BracketSignType.LeftDoubleBar, BracketSignType.RightDoubleBar},
            new BracketSignType [] {BracketSignType.LeftFloor,     BracketSignType.RightFloor},
            new BracketSignType [] {BracketSignType.LeftCeiling,   BracketSignType.RightCeiling},

            new BracketSignType [] {BracketSignType.LeftSquare,    BracketSignType.RightRound},
            new BracketSignType [] {BracketSignType.LeftRound,     BracketSignType.RightSquare},
            new BracketSignType [] {BracketSignType.LeftBar,       BracketSignType.RightAngle},
            new BracketSignType [] {BracketSignType.LeftAngle,     BracketSignType.RightBar},

            new BracketSignType [] {BracketSignType.LeftSquare,    BracketSignType.LeftSquare},
            new BracketSignType [] {BracketSignType.RightSquare,   BracketSignType.RightSquare},
            new BracketSignType [] {BracketSignType.RightSquare,   BracketSignType.LeftSquare},
            new BracketSignType [] {BracketSignType.LeftSquareBar, BracketSignType.RightSquareBar},

            BracketSignType.LeftRound,
            BracketSignType.RightRound,
            BracketSignType.LeftSquare,
            BracketSignType.RightSquare,

            BracketSignType.LeftCurly,
            BracketSignType.RightCurly,
            BracketSignType.LeftAngle,
            BracketSignType.RightAngle,

            BracketSignType.LeftBar,
            BracketSignType.RightBar,
            BracketSignType.LeftDoubleBar,
            BracketSignType.RightDoubleBar,

            BracketSignType.LeftSquareBar,
            BracketSignType.RightSquareBar,
            0,
            0,  //empty cell

            HorizontalBracketSignType.TopCurly,
            HorizontalBracketSignType.BottomCurly,
            HorizontalBracketSignType.TopSquare,
            HorizontalBracketSignType.BottomSquare,
        ];

        CreateImagePanel(imageUris, commands, paramz, bracketsButton, 4);
    }

    private void CreateSumsProductsPanel()
    {
        string[] imageUris = [
            CreateImagePath("SumsProducts", "sum.png"),
            CreateImagePath("SumsProducts", "sumSub.png"),
            CreateImagePath("SumsProducts", "sumSubSuper.png"),
            CreateImagePath("SumsProducts", "sumBottom.png"),
            CreateImagePath("SumsProducts", "sumBottomTop.png"),

            CreateImagePath("SumsProducts", "product.png"),
            CreateImagePath("SumsProducts", "productSub.png"),
            CreateImagePath("SumsProducts", "productSubSuper.png"),
            CreateImagePath("SumsProducts", "productBottom.png"),
            CreateImagePath("SumsProducts", "productBottomTop.png"),

            CreateImagePath("SumsProducts", "coProduct.png"),
            CreateImagePath("SumsProducts", "coProductSub.png"),
            CreateImagePath("SumsProducts", "coProductSubSuper.png"),
            CreateImagePath("SumsProducts", "coProductBottom.png"),
            CreateImagePath("SumsProducts", "coProductBottomTop.png"),

            CreateImagePath("SumsProducts", "intersection.png"),
            CreateImagePath("SumsProducts", "intersectionSub.png"),
            CreateImagePath("SumsProducts", "intersectionSubSuper.png"),
            CreateImagePath("SumsProducts", "intersectionBottom.png"),
            CreateImagePath("SumsProducts", "intersectionBottomTop.png"),

            CreateImagePath("SumsProducts", "union.png"),
            CreateImagePath("SumsProducts", "unionSub.png"),
            CreateImagePath("SumsProducts", "unionSubSuper.png"),
            CreateImagePath("SumsProducts", "unionBottom.png"),
            CreateImagePath("SumsProducts", "unionBottomTop.png"),
        ];
        CommandType[] commands = [.. Enumerable.Repeat(CommandType.SignComposite, imageUris.Length)];
        object[] paramz = [
            new object [] {Position.None,    SignCompositeSymbol.Sum} ,
            new object [] {Position.Sub,       SignCompositeSymbol.Sum} ,
            new object [] {Position.SubAndSuper,  SignCompositeSymbol.Sum} ,
            new object [] {Position.Bottom,    SignCompositeSymbol.Sum} ,
            new object [] {Position.BottomAndTop, SignCompositeSymbol.Sum} ,

            new object [] {Position.None,    SignCompositeSymbol.Product} ,
            new object [] {Position.Sub,       SignCompositeSymbol.Product} ,
            new object [] {Position.SubAndSuper,  SignCompositeSymbol.Product} ,
            new object [] {Position.Bottom,    SignCompositeSymbol.Product} ,
            new object [] {Position.BottomAndTop, SignCompositeSymbol.Product} ,

            new object [] {Position.None,    SignCompositeSymbol.CoProduct} ,
            new object [] {Position.Sub,       SignCompositeSymbol.CoProduct} ,
            new object [] {Position.SubAndSuper,  SignCompositeSymbol.CoProduct} ,
            new object [] {Position.Bottom,    SignCompositeSymbol.CoProduct} ,
            new object [] {Position.BottomAndTop, SignCompositeSymbol.CoProduct} ,

            new object [] {Position.None,    SignCompositeSymbol.Intersection} ,
            new object [] {Position.Sub,       SignCompositeSymbol.Intersection} ,
            new object [] {Position.SubAndSuper,  SignCompositeSymbol.Intersection} ,
            new object [] {Position.Bottom,    SignCompositeSymbol.Intersection} ,
            new object [] {Position.BottomAndTop, SignCompositeSymbol.Intersection} ,

            new object [] {Position.None,    SignCompositeSymbol.Union} ,
            new object [] {Position.Sub,       SignCompositeSymbol.Union} ,
            new object [] {Position.SubAndSuper,  SignCompositeSymbol.Union} ,
            new object [] {Position.Bottom,    SignCompositeSymbol.Union} ,
            new object [] {Position.BottomAndTop, SignCompositeSymbol.Union} ,
        ];

        CreateImagePanel(imageUris, commands, paramz, sumsProductsButton, 5);
    }

    private void CreateIntegralsPanel()
    {
        string[] imageUris = [
            CreateImagePath("Integrals/Single", "Simple.png"),
            CreateImagePath("Integrals/Single", "Sub.png"),
            CreateImagePath("Integrals/Single", "SubSuper.png"),
            CreateImagePath("Integrals/Single", "Bottom.png"),
            CreateImagePath("Integrals/Single", "BottomTop.png"),

            CreateImagePath("Integrals/Double", "Simple.png"),
            CreateImagePath("Integrals/Double", "Sub.png"),
            CreateImagePath("Integrals/Double", "SubSuper.png"),
            CreateImagePath("Integrals/Double", "Bottom.png"),
            CreateImagePath("Integrals/Double", "BottomTop.png"),

            CreateImagePath("Integrals/Triple", "Simple.png"),
            CreateImagePath("Integrals/Triple", "Sub.png"),
            CreateImagePath("Integrals/Triple", "SubSuper.png"),
            CreateImagePath("Integrals/Triple", "Bottom.png"),
            CreateImagePath("Integrals/Triple", "BottomTop.png"),

            CreateImagePath("Integrals/Contour", "Simple.png"),
            CreateImagePath("Integrals/Contour", "Sub.png"),
            CreateImagePath("Integrals/Contour", "SubSuper.png"),
            CreateImagePath("Integrals/Contour", "Bottom.png"),
            CreateImagePath("Integrals/Contour", "BottomTop.png"),

            CreateImagePath("Integrals/Surface", "Simple.png"),
            CreateImagePath("Integrals/Surface", "Sub.png"),
            CreateImagePath("Integrals/Surface", "SubSuper.png"),
            CreateImagePath("Integrals/Surface", "Bottom.png"),
            CreateImagePath("Integrals/Surface", "BottomTop.png"),

            CreateImagePath("Integrals/Volume", "Simple.png"),
            CreateImagePath("Integrals/Volume", "Sub.png"),
            CreateImagePath("Integrals/Volume", "SubSuper.png"),
            CreateImagePath("Integrals/Volume", "Bottom.png"),
            CreateImagePath("Integrals/Volume", "BottomTop.png"),

            CreateImagePath("Integrals/Clock", "Simple.png"),
            CreateImagePath("Integrals/Clock", "Sub.png"),
            CreateImagePath("Integrals/Clock", "SubSuper.png"),
            CreateImagePath("Integrals/Clock", "Bottom.png"),
            CreateImagePath("Integrals/Clock", "BottomTop.png"),

            CreateImagePath("Integrals/AntiClock", "Simple.png"),
            CreateImagePath("Integrals/AntiClock", "Sub.png"),
            CreateImagePath("Integrals/AntiClock", "SubSuper.png"),
            CreateImagePath("Integrals/AntiClock", "Bottom.png"),
            CreateImagePath("Integrals/AntiClock", "BottomTop.png"),
        ];
        CommandType[] commands = [.. Enumerable.Repeat(CommandType.SignComposite, imageUris.Length)];
        object[] paramz = [
            new object [] {Position.None,    SignCompositeSymbol.Integral},
            new object [] {Position.Sub,       SignCompositeSymbol.Integral},
            new object [] {Position.SubAndSuper,  SignCompositeSymbol.Integral},
            new object [] {Position.Bottom,    SignCompositeSymbol.Integral},
            new object [] {Position.BottomAndTop, SignCompositeSymbol.Integral},

            new object [] {Position.None,    SignCompositeSymbol.DoubleIntegral},
            new object [] {Position.Sub,       SignCompositeSymbol.DoubleIntegral},
            new object [] {Position.SubAndSuper,  SignCompositeSymbol.DoubleIntegral},
            new object [] {Position.Bottom,    SignCompositeSymbol.DoubleIntegral},
            new object [] {Position.BottomAndTop, SignCompositeSymbol.DoubleIntegral},

            new object [] {Position.None,    SignCompositeSymbol.TripleIntegral},
            new object [] {Position.Sub,       SignCompositeSymbol.TripleIntegral},
            new object [] {Position.SubAndSuper,  SignCompositeSymbol.TripleIntegral},
            new object [] {Position.Bottom,    SignCompositeSymbol.TripleIntegral},
            new object [] {Position.BottomAndTop, SignCompositeSymbol.TripleIntegral},

            new object [] {Position.None,    SignCompositeSymbol.ContourIntegral},
            new object [] {Position.Sub,       SignCompositeSymbol.ContourIntegral},
            new object [] {Position.SubAndSuper,  SignCompositeSymbol.ContourIntegral},
            new object [] {Position.Bottom,    SignCompositeSymbol.ContourIntegral},
            new object [] {Position.BottomAndTop, SignCompositeSymbol.ContourIntegral},

            new object [] {Position.None,    SignCompositeSymbol.SurfaceIntegral},
            new object [] {Position.Sub,       SignCompositeSymbol.SurfaceIntegral},
            new object [] {Position.SubAndSuper,  SignCompositeSymbol.SurfaceIntegral},
            new object [] {Position.Bottom,    SignCompositeSymbol.SurfaceIntegral},
            new object [] {Position.BottomAndTop, SignCompositeSymbol.SurfaceIntegral},

            new object [] {Position.None,    SignCompositeSymbol.VolumeIntegral},
            new object [] {Position.Sub,       SignCompositeSymbol.VolumeIntegral},
            new object [] {Position.SubAndSuper,  SignCompositeSymbol.VolumeIntegral},
            new object [] {Position.Bottom,    SignCompositeSymbol.VolumeIntegral},
            new object [] {Position.BottomAndTop, SignCompositeSymbol.VolumeIntegral},

            new object [] {Position.None,    SignCompositeSymbol.ClockContourIntegral},
            new object [] {Position.Sub,       SignCompositeSymbol.ClockContourIntegral},
            new object [] {Position.SubAndSuper,  SignCompositeSymbol.ClockContourIntegral},
            new object [] {Position.Bottom,    SignCompositeSymbol.ClockContourIntegral},
            new object [] {Position.BottomAndTop, SignCompositeSymbol.ClockContourIntegral},

            new object [] {Position.None,    SignCompositeSymbol.AntiClockContourIntegral},
            new object [] {Position.Sub,       SignCompositeSymbol.AntiClockContourIntegral},
            new object [] {Position.SubAndSuper,  SignCompositeSymbol.AntiClockContourIntegral},
            new object [] {Position.Bottom,    SignCompositeSymbol.AntiClockContourIntegral},
            new object [] {Position.BottomAndTop, SignCompositeSymbol.AntiClockContourIntegral},
        ];

        CreateImagePanel(imageUris, commands, paramz, integralsButton, 5);
    }

    private void CreateSubAndSuperPanel()
    {
        string[] imageUris = [
            CreateImagePath("SubSuper", "Sub.png"),
            CreateImagePath("SubSuper", "Super.png"),
            CreateImagePath("SubSuper", "SubSuper.png"),
            CreateImagePath("SubSuper", "SubLeft.png"),
            CreateImagePath("SubSuper", "SuperLeft.png"),
            CreateImagePath("SubSuper", "SubSuperLeft.png"),
        ];
        CommandType[] commands = [ CommandType.Sub, CommandType.Super, CommandType.SubAndSuper,
                                   CommandType.Sub, CommandType.Super, CommandType.SubAndSuper];
        object[] paramz = [ Position.Right, Position.Right, Position.Right,
                            Position.Left, Position.Left, Position.Left,];

        CreateImagePanel(imageUris, commands, paramz, subAndSuperButton, 3);
    }

    private void CreateCompositePanel()
    {
        string[] imageUris = [
            CreateImagePath("Composite", "CompositeBottom.png"),
            CreateImagePath("Composite", "CompositeTop.png"),
            CreateImagePath("Composite", "CompositeBottomTop.png"),
            CreateImagePath("Composite", "BigBottom.png"),
            CreateImagePath("Composite", "BigTop.png"),
            CreateImagePath("Composite", "BigBottomTop.png"),
            CreateImagePath("Composite", "BigSub.png"),
            CreateImagePath("Composite", "BigSuper.png"),
            CreateImagePath("Composite", "BigSubSuper.png"),
        ];
        CommandType[] commands = [
            CommandType.Composite, CommandType.Composite, CommandType.Composite,
            CommandType.CompositeBig, CommandType.CompositeBig, CommandType.CompositeBig,
            CommandType.CompositeBig, CommandType.CompositeBig, CommandType.CompositeBig,
        ];
        object[] paramz = [
            Position.Bottom, Position.Top, Position.BottomAndTop,
            Position.Bottom, Position.Top, Position.BottomAndTop,
            Position.Sub, Position.Super, Position.SubAndSuper,
        ];

        CreateImagePanel(imageUris, commands, paramz, compositeButton, 3);
    }

    private void CreateDecoratedEquationPanel()
    {
        string[] imageUris = [
            CreateImagePath("Decorated/Equation", "tilde.png"),
            CreateImagePath("Decorated/Equation", "hat.png"),
            CreateImagePath("Decorated/Equation", "parenthesis.png"),
            CreateImagePath("Decorated/Equation", "tortoise.png"),
            CreateImagePath("Decorated/Equation", "topBar.png"),
            CreateImagePath("Decorated/Equation", "topDoubleBar.png"),
            CreateImagePath("Decorated/Equation", "topRightArrow.png"),
            CreateImagePath("Decorated/Equation", "topLeftArrow.png"),
            CreateImagePath("Decorated/Equation", "topRightHalfArrow.png"),
            CreateImagePath("Decorated/Equation", "topLeftHalfArrow.png"),
            CreateImagePath("Decorated/Equation", "topDoubleArrow.png"),

            CreateImagePath("Decorated/Equation", "topDoubleArrow.png"),  //to be left empty

            CreateImagePath("Decorated/Equation", "bottomBar.png"),
            CreateImagePath("Decorated/Equation", "bottomDoubleBar.png"),
            CreateImagePath("Decorated/Equation", "bottomRightArrow.png"),
            CreateImagePath("Decorated/Equation", "bottomLeftArrow.png"),
            CreateImagePath("Decorated/Equation", "bottomRightHalfArrow.png"),
            CreateImagePath("Decorated/Equation", "bottomLeftHalfArrow.png"),
            CreateImagePath("Decorated/Equation", "bottomDoubleArrow.png"),

            CreateImagePath("Decorated/Equation", "bottomDoubleArrow.png"),  //to be left empty
                              
            CreateImagePath("Decorated/Equation", "strikeThrough.png"),
            CreateImagePath("Decorated/Equation", "cross.png"),
            CreateImagePath("Decorated/Equation", "rightCross.png"),
            CreateImagePath("Decorated/Equation", "leftCross.png"),
        ];
        CommandType[] commands = [.. Enumerable.Repeat(CommandType.Decorated, imageUris.Length)];
        commands[11] = CommandType.None; //empty cell
        commands[19] = CommandType.None; //empty cell
        object[] paramz = [
            new object [] {DecorationType.Tilde,                  Position.Top },
            new object [] {DecorationType.Hat,                    Position.Top },
            new object [] {DecorationType.Parenthesis,            Position.Top },
            new object [] {DecorationType.Tortoise,               Position.Top },
            new object [] {DecorationType.Bar,                    Position.Top },
            new object [] {DecorationType.DoubleBar,              Position.Top },
            new object [] {DecorationType.RightArrow,             Position.Top },
            new object [] {DecorationType.LeftArrow,              Position.Top },
            new object [] {DecorationType.RightHarpoonUpBarb,     Position.Top },
            new object [] {DecorationType.LeftHarpoonUpBarb,      Position.Top },
            new object [] {DecorationType.DoubleArrow,            Position.Top },
            0, //empty cell                                  
            new object [] {DecorationType.Bar,                    Position.Bottom },
            new object [] {DecorationType.DoubleBar,              Position.Bottom },
            new object [] {DecorationType.RightArrow,             Position.Bottom },
            new object [] {DecorationType.LeftArrow,              Position.Bottom },
            new object [] {DecorationType.RightHarpoonDownBarb,   Position.Bottom },
            new object [] {DecorationType.LeftHarpoonDownBarb,    Position.Bottom },
            new object [] {DecorationType.DoubleArrow,            Position.Bottom },
            0, //empty cell
            new object [] {DecorationType.StrikeThrough,  Position.Middle },
            new object [] {DecorationType.Cross,          Position.Middle },
            new object [] {DecorationType.RightCross,     Position.Middle },
            new object [] {DecorationType.LeftCross,      Position.Middle },
        ];
        CreateImagePanel(imageUris, commands, paramz, decoratedEquationButton, 4);
    }

    private void CreateDecoratedCharacterPanel()
    {
        string[] imageUris = [
            CreateImagePath("Decorated/Character", "None.png"),
            CreateImagePath("Decorated/Character", "StrikeThrough.png"),
            CreateImagePath("Decorated/Character", "DoubleStrikeThrough.png"),
            CreateImagePath("Decorated/Character", "LeftCross.png"),
            CreateImagePath("Decorated/Character", "RightCross.png"),
            CreateImagePath("Decorated/Character", "Cross.png"),
            CreateImagePath("Decorated/Character", "VstrikeThrough.png"),
            CreateImagePath("Decorated/Character", "VDoubleStrikeThrough.png"),
            CreateImagePath("Decorated/Character", "LeftUprightCross.png"),
            CreateImagePath("Decorated/Character", "RightUprightCross.png"),

            CreateImagePath("Decorated/Character", "Prime.png"),
            CreateImagePath("Decorated/Character", "DoublePrime.png"),
            CreateImagePath("Decorated/Character", "TriplePrime.png"),
            CreateImagePath("Decorated/Character", "ReversePrime.png"),
            CreateImagePath("Decorated/Character", "ReverseDoublePrime.png"),

            CreateImagePath("Decorated/Character", "AcuteAccent.png"),
            CreateImagePath("Decorated/Character", "GraveAccent.png"),
            CreateImagePath("Decorated/Character", "TopRing.png"),
            CreateImagePath("Decorated/Character", "TopRightRing.png"),
            CreateImagePath("Decorated/Character", "ReverseDoublePrime.png"), //Empty

            CreateImagePath("Decorated/Character", "TopBar.png"),
            CreateImagePath("Decorated/Character", "TopTilde.png"),
            CreateImagePath("Decorated/Character", "TopBreve.png"),
            CreateImagePath("Decorated/Character", "TopInvertedBreve.png"),
            CreateImagePath("Decorated/Character", "TopCircumflex.png"),

            CreateImagePath("Decorated/Character", "BottomBar.png"),
            CreateImagePath("Decorated/Character", "BottomTilde.png"),
            CreateImagePath("Decorated/Character", "BottomBreve.png"),
            CreateImagePath("Decorated/Character", "BottomInvertedBreve.png"),
            CreateImagePath("Decorated/Character", "TopCaron.png"),

            CreateImagePath("Decorated/Character", "TopRightArrow.png"),
            CreateImagePath("Decorated/Character", "TopLeftArrow.png"),
            CreateImagePath("Decorated/Character", "TopDoubleArrow.png"),
            CreateImagePath("Decorated/Character", "TopRightHarpoon.png"),
            CreateImagePath("Decorated/Character", "TopLeftHarpoon.png"),

            CreateImagePath("Decorated/Character", "BottomRightArrow.png"),
            CreateImagePath("Decorated/Character", "BottomLeftArrow.png"),
            CreateImagePath("Decorated/Character", "BottomDoubleArrow.png"),
            CreateImagePath("Decorated/Character", "BottomRightHarpoon.png"),
            CreateImagePath("Decorated/Character", "BottomLeftHarpoon.png"),

            CreateImagePath("Decorated/Character", "TopDot.png"),
            CreateImagePath("Decorated/Character", "TopDDot.png"),
            CreateImagePath("Decorated/Character", "TopTDot.png"),
            CreateImagePath("Decorated/Character", "TopFourDot.png"),
            CreateImagePath("Decorated/Character", "TopFourDot.png"), //Empty
                              
            CreateImagePath("Decorated/Character", "BottomDot.png"),
            CreateImagePath("Decorated/Character", "BottomDDot.png"),
            CreateImagePath("Decorated/Character", "BottomTDot.png"),
            CreateImagePath("Decorated/Character", "BottomFourDot.png"),
            CreateImagePath("Decorated/Character", "BottomFourDot.png"), //Empty
        ];
        CommandType[] commands = [.. Enumerable.Repeat(CommandType.DecoratedCharacter, imageUris.Length)];
        commands[19] = CommandType.None; //empty cell 
        commands[44] = CommandType.None; //empty cell           
        commands[49] = CommandType.None; //empty cell  

        object[] paramz = [
            new object [] {CharacterDecorationType.None,                  Position.Over, null!},
            new object [] {CharacterDecorationType.StrikeThrough,         Position.Over, null!},
            new object [] {CharacterDecorationType.DoubleStrikeThrough,   Position.Over, null!},
            new object [] {CharacterDecorationType.LeftCross,             Position.Over, null!},
            new object [] {CharacterDecorationType.RightCross,            Position.Over, null!},
            new object [] {CharacterDecorationType.Cross,                 Position.Over, null!},
            new object [] {CharacterDecorationType.VStrikeThrough,        Position.Over, null!},
            new object [] {CharacterDecorationType.VDoubleStrikeThrough,  Position.Over, null!},
            new object [] {CharacterDecorationType.LeftUprightCross,      Position.Over, null!},
            new object [] {CharacterDecorationType.RightUprightCross,     Position.Over, null!},

            new object [] {CharacterDecorationType.Unicode, Position.TopRight,  "\u2032"}, //Prime
            new object [] {CharacterDecorationType.Unicode, Position.TopRight,  "\u2033"}, //Double prime
            new object [] {CharacterDecorationType.Unicode, Position.TopRight,  "\u2034"}, //Triple prime
            new object [] {CharacterDecorationType.Unicode, Position.TopLeft,   "\u2035"}, //Reversed prime
            new object [] {CharacterDecorationType.Unicode, Position.TopLeft,   "\u2036"}, //Double reversed prime

            new object [] {CharacterDecorationType.Unicode, Position.Top,  "\u02CA"}, // Acute
            new object [] {CharacterDecorationType.Unicode, Position.Top,  "\u02CB"}, //Grave
            new object [] {CharacterDecorationType.Unicode, Position.Top,  "\u030A"}, //Ring
            new object [] {CharacterDecorationType.Unicode, Position.TopRight,  "\u030A"}, //Ring
            0, //Empty
                              
            new object [] {CharacterDecorationType.Unicode, Position.Top,  "\u0332"}, //Bar or line
            new object [] {CharacterDecorationType.Unicode, Position.Top,  "\u0334"}, //Tilde
            new object [] {CharacterDecorationType.Unicode, Position.Top, "\u0306"}, //Breve
            new object [] {CharacterDecorationType.Unicode, Position.Top, "\u0311"}, //Inverted Breve
            new object [] {CharacterDecorationType.Unicode, Position.Top, "\u02C6"}, //Circumflex

            new object [] {CharacterDecorationType.Unicode, Position.Bottom, "\u0332"}, //Bar or line
            new object [] {CharacterDecorationType.Unicode, Position.Bottom, "\u0334"}, //Tilde
            new object [] {CharacterDecorationType.Unicode, Position.Bottom, "\u0306"}, //Breve
            new object [] {CharacterDecorationType.Unicode, Position.Bottom, "\u0311"}, //Inverted breve
            new object [] {CharacterDecorationType.Unicode, Position.Top, "\u02C7"}, //Caron or check

            new object [] {CharacterDecorationType.Unicode, Position.Top, "\u20D7"}, //left arrow
            new object [] {CharacterDecorationType.Unicode, Position.Top, "\u20D6"}, //right arrow
            new object [] {CharacterDecorationType.Unicode, Position.Top, "\u20E1"}, //double arrow
            new object [] {CharacterDecorationType.Unicode, Position.Top, "\u20D1"}, //top right harpoon
            new object [] {CharacterDecorationType.Unicode, Position.Top, "\u20D0"}, //top left harpoon

            new object [] {CharacterDecorationType.Unicode, Position.Bottom, "\u20D7"}, //left arrow
            new object [] {CharacterDecorationType.Unicode, Position.Bottom, "\u20D6"}, //right arrow
            new object [] {CharacterDecorationType.Unicode, Position.Bottom, "\u20E1"}, //double arrow
            new object [] {CharacterDecorationType.Unicode, Position.Bottom, "\u20EC"}, //bottom right harpoon
            new object [] {CharacterDecorationType.Unicode, Position.Bottom, "\u20ED"}, //bottom left harpoon

            new object [] {CharacterDecorationType.Unicode, Position.Top, "\u0323"},  //dot
            new object [] {CharacterDecorationType.Unicode, Position.Top, "\u0324"},  //two dots
            new object [] {CharacterDecorationType.Unicode, Position.Top, "\u20DB" }, //three dots
            new object [] {CharacterDecorationType.Unicode, Position.Top, "\u20DC" }, //four dots
            0, //Empty
            new object [] {CharacterDecorationType.Unicode, Position.Bottom, "\u0323"},  //dot
            new object [] {CharacterDecorationType.Unicode, Position.Bottom, "\u0324"},  //two dots
            new object [] {CharacterDecorationType.Unicode, Position.Bottom, "\u20DB" }, //three dots
            new object [] {CharacterDecorationType.Unicode, Position.Bottom, "\u20DC" }, //four dots
            0, //Empty
        ];

        CreateImagePanel(imageUris, commands, paramz, decoratedCharacterButton, 5);
    }

    private void CreateArrowEquationPanel()
    {
        string[] imageUris = [
            CreateImagePath("Decorated/Arrow", "LeftTop.png"),
            CreateImagePath("Decorated/Arrow", "LeftBottom.png"),
            CreateImagePath("Decorated/Arrow", "LeftBottomTop.png"),

            CreateImagePath("Decorated/Arrow", "RightTop.png"),
            CreateImagePath("Decorated/Arrow", "RightBottom.png"),
            CreateImagePath("Decorated/Arrow", "RightBottomTop.png"),

            CreateImagePath("Decorated/Arrow", "DoubleTop.png"),
            CreateImagePath("Decorated/Arrow", "DoubleBottom.png"),
            CreateImagePath("Decorated/Arrow", "DoubleBottomTop.png"),

            CreateImagePath("Decorated/Arrow", "RightLeftTop.png"),
            CreateImagePath("Decorated/Arrow", "RightLeftBottom.png"),
            CreateImagePath("Decorated/Arrow", "RightLeftBottomTop.png"),

            CreateImagePath("Decorated/Arrow", "RightSmallLeftTop.png"),
            CreateImagePath("Decorated/Arrow", "RightSmallLeftBottom.png"),
            CreateImagePath("Decorated/Arrow", "RightSmallLeftBottomTop.png"),

            CreateImagePath("Decorated/Arrow", "SmallRightLeftTop.png"),
            CreateImagePath("Decorated/Arrow", "SmallRightLeftBottom.png"),
            CreateImagePath("Decorated/Arrow", "SmallRightLeftBottomTop.png"),

            CreateImagePath("Decorated/Arrow", "RightLeftHarpTop.png"),
            CreateImagePath("Decorated/Arrow", "RightLeftHarpBottom.png"),
            CreateImagePath("Decorated/Arrow", "RightLeftHarpBottomTop.png"),

            CreateImagePath("Decorated/Arrow", "RightSmallLeftHarpTop.png"),
            CreateImagePath("Decorated/Arrow", "RightSmallLeftHarpBottom.png"),
            CreateImagePath("Decorated/Arrow", "RightSmallLeftHarpBottomTop.png"),

            CreateImagePath("Decorated/Arrow", "SmallRightLeftHarpTop.png"),
            CreateImagePath("Decorated/Arrow", "SmallRightLeftHarpBottom.png"),
            CreateImagePath("Decorated/Arrow", "SmallRightLeftHarpBottomTop.png"),
        ];
        CommandType[] commands = [.. Enumerable.Repeat(CommandType.Arrow, imageUris.Length)];
        object[] paramz = [
            new object [] {ArrowType.LeftArrow,               Position.Top },
            new object [] {ArrowType.LeftArrow,               Position.Bottom },
            new object [] {ArrowType.LeftArrow,               Position.BottomAndTop },

            new object [] {ArrowType.RightArrow,              Position.Top },
            new object [] {ArrowType.RightArrow,              Position.Bottom },
            new object [] {ArrowType.RightArrow,              Position.BottomAndTop },

            new object [] {ArrowType.DoubleArrow,             Position.Top },
            new object [] {ArrowType.DoubleArrow,             Position.Bottom },
            new object [] {ArrowType.DoubleArrow,             Position.BottomAndTop },

            new object [] {ArrowType.RightLeftArrow,          Position.Top },
            new object [] {ArrowType.RightLeftArrow,          Position.Bottom },
            new object [] {ArrowType.RightLeftArrow,          Position.BottomAndTop },

            new object [] {ArrowType.RightSmallLeftArrow,     Position.Top },
            new object [] {ArrowType.RightSmallLeftArrow,     Position.Bottom },
            new object [] {ArrowType.RightSmallLeftArrow,     Position.BottomAndTop },

            new object [] {ArrowType.SmallRightLeftArrow,     Position.Top },
            new object [] {ArrowType.SmallRightLeftArrow,     Position.Bottom },
            new object [] {ArrowType.SmallRightLeftArrow,     Position.BottomAndTop },

            new object [] {ArrowType.RightLeftHarpoon,        Position.Top },
            new object [] {ArrowType.RightLeftHarpoon,        Position.Bottom },
            new object [] {ArrowType.RightLeftHarpoon,        Position.BottomAndTop },

            new object [] {ArrowType.RightSmallLeftHarpoon,     Position.Top },
            new object [] {ArrowType.RightSmallLeftHarpoon,     Position.Bottom },
            new object [] {ArrowType.RightSmallLeftHarpoon,     Position.BottomAndTop },

            new object [] {ArrowType.SmallRightLeftHarpoon,    Position.Top },
            new object [] {ArrowType.SmallRightLeftHarpoon,    Position.Bottom },
            new object [] {ArrowType.SmallRightLeftHarpoon,    Position.BottomAndTop },
        ];
        CreateImagePanel(imageUris, commands, paramz, arrowEquationButton, 3);
    }

    private void CreateDivAndRootsPanel()
    {
        string[] imageUris = [
            CreateImagePath("DivAndRoots", "SqRoot.png"),
            CreateImagePath("DivAndRoots", "nRoot.png"),
            CreateImagePath("DivAndRoots", "DivMath.png"),
            CreateImagePath("DivAndRoots", "DivMathWithTop.png"),

            CreateImagePath("DivAndRoots", "Div.png"),
            CreateImagePath("DivAndRoots", "DivDoubleBar.png"),
            CreateImagePath("DivAndRoots", "DivTripleBar.png"),
            CreateImagePath("DivAndRoots", "SmallDiv.png"),

            CreateImagePath("DivAndRoots", "DivSlant.png"),
            CreateImagePath("DivAndRoots", "SmallDivSlant.png"),
            CreateImagePath("DivAndRoots", "DivHoriz.png"),
            CreateImagePath("DivAndRoots", "SmallDivHoriz.png"),

            CreateImagePath("DivAndRoots", "DivMathInverted.png"),
            CreateImagePath("DivAndRoots", "DivMathInvertedWithBottom.png"),
            CreateImagePath("DivAndRoots", "DivTriangleFixed.png"),
            CreateImagePath("DivAndRoots", "DivTriangleExpanding.png"),
        ];
        CommandType[] commands = [
            CommandType.SquareRoot, CommandType.nRoot,
            CommandType.Division, CommandType.Division, CommandType.Division,
            CommandType.Division, CommandType.Division, CommandType.Division,
            CommandType.Division, CommandType.Division, CommandType.Division,
            CommandType.Division, CommandType.Division, CommandType.Division,
            CommandType.Division, CommandType.Division,
        ];
        object[] paramz = [
            0, 0, //square root and nRoot
            DivisionType.DivMath, DivisionType.DivMathWithTop,
            DivisionType.DivRegular, DivisionType.DivDoubleBar, DivisionType.DivTripleBar,
            DivisionType.DivRegularSmall, DivisionType.DivSlanted, DivisionType.DivSlantedSmall,
            DivisionType.DivHoriz, DivisionType.DivHorizSmall, DivisionType.DivMathInverted,
            DivisionType.DivInvertedWithBottom, DivisionType.DivTriangleFixed,
            DivisionType.DivTriangleExpanding,
        ];

        CreateImagePanel(imageUris, commands, paramz, divAndRootsButton, 4);
    }

    private void CreateBoxEquationPanel()
    {
        string[] imageUris = [
            CreateImagePath("Box", "leftTop.png"),
            CreateImagePath("Box", "leftBottom.png"),
            CreateImagePath("Box", "rightTop.png"),
            CreateImagePath("Box", "rightBottom.png"),
            CreateImagePath("Box", "all.png"),
        ];
        CommandType[] commands = [.. Enumerable.Repeat(CommandType.Box, imageUris.Length)];
        object[] paramz = [BoxType.LeftTop, BoxType.LeftBottom, BoxType.RightTop, BoxType.RightBottom, BoxType.All];
        CreateImagePanel(imageUris, commands, paramz, boxButton, 2);
    }

    private void CreateMatrixPanel()
    {
        string[] imageUris = [
            CreateImagePath("Matrix", "2cellRow.png"),
            CreateImagePath("Matrix", "2cellColumn.png"),
            CreateImagePath("Matrix", "2Square.png"),

            CreateImagePath("Matrix", "3cellRow.png"),
            CreateImagePath("Matrix", "3cellColumn.png"),
            CreateImagePath("Matrix", "3Square.png"),

            CreateImagePath("Matrix", "row.png"),
            CreateImagePath("Matrix", "column.png"),
            CreateImagePath("Matrix", "custom.png"),
        ];
        CommandType[] commands = [.. Enumerable.Repeat(CommandType.Matrix, imageUris.Length)];
        commands[6] = CommandType.CustomMatrix;
        commands[7] = CommandType.CustomMatrix;
        commands[8] = CommandType.CustomMatrix;
        object[] paramz = [
            new [] {1, 2},
            new [] {2, 1},
            new [] {2, 2},
            new [] {1, 3},
            new [] {3, 1},
            new [] {3, 3},
            new [] {1, 4},
            new [] {4, 1},
            new [] {4, 4},
        ];

        CreateImagePanel(imageUris, commands, paramz, matrixButton, 3);
    }
}
