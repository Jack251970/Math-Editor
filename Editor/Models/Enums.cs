using Editor.Localization.Attributes;

namespace Editor;

public enum CommandType
{
    None,
    Text,
    ShowBox, HideBox,
    SquareRoot, nRoot, Division,
    LeftBracket, RightBracket, LeftRightBracket,
    Sub, Super, SubAndSuper, SignComposite, Composite, CompositeBig,
    TopBracket, BottomBracket, DoubleArrowBarBracket,
    Decorated, Arrow, Box, Matrix, CustomMatrix, DecoratedCharacter
}

public enum HAlignment { Left, Center, Right }
public enum VAlignment { Center, Top, Bottom }

public enum Position
{
    None, Middle, Top, Bottom, Left, Right, Sub, Super, SubAndSuper, BottomAndTop,
    TopLeft, BottomLeft, TopRight, BottomRight, Over
}

public enum CharacterDecorationType
{
    None,
    StrikeThrough, DoubleStrikeThrough, VStrikeThrough,
    VDoubleStrikeThrough, Cross, LeftCross, RightCross,
    LeftUprightCross, RightUprightCross,
    Unicode,
}


//public enum CharacterDecorationType
//{
//    StrikeThrough, DoubleStrikeThrough, HStrikeThrough, HDoubleStrikeThrough, Cross, LeftCross, RightCross,

//    SuperRing, TopRing, TopBar,
//    RightArrowTop, LeftArrowTop, DoubleArrowTop,
//    LeftHarpoonTop, RightHarpoonTop,
//    Prime, DoublePrime, TriplePrime,
//    TopDot, TopDoubleDot, TopTripleDot, TopFourDots,
//    TopVel, TopHat, TopBreve,
//    Accute, Grave, GraveLeft, TopCircumflex,

//    BottomDot, BottomDoubleDot, BottomTripleDot, BottomFourDots,
//    BottomBar, BottomTilde, BottomBreve, BottomCircumflex,
//    BottomRightArrow, BottomLeftArrow, BottomDoubleArrow,
//    BottomRightHarpoon, BottomLeftHarpoon,
//}

public enum DivisionType
{
    DivRegular, DivDoubleBar, DivTripleBar,
    DivMath, DivMathWithTop,
    DivHoriz, DivSlanted,
    DivRegularSmall, DivHorizSmall, DivSlantedSmall,
    DivMathInverted, DivInvertedWithBottom,
    DivTriangleFixed, DivTriangleExpanding
}

public enum BoxType
{
    All, LeftTop, RightTop, LeftBottom, RightBottom
}

public enum ArrowType
{
    RightArrow, LeftArrow, DoubleArrow, RightLeftArrow, RightSmallLeftArrow, SmallRightLeftArrow,
    RightLeftHarpoon, RightSmallLeftHarpoon, SmallRightLeftHarpoon
}

public enum DecorationType
{
    Tilde, Hat, Parenthesis, Tortoise,
    Bar, DoubleBar, RightArrow, LeftArrow,
    RightHarpoonUpBarb, LeftHarpoonUpBarb, DoubleArrow,
    RightHarpoonDownBarb, LeftHarpoonDownBarb,
    StrikeThrough, Cross, RightCross, LeftCross
}

public enum SignCompositeSymbol
{
    Sum, Product, CoProduct, Intersection, Union, Integral, DoubleIntegral, TripleIntegral,
    ContourIntegral, SurfaceIntegral, VolumeIntegral, ClockContourIntegral, AntiClockContourIntegral
}

public enum IntegralType
{
    Integral, DoubleIntegral, TripleIntegral,
    ContourIntegral,
    SurfaceIntegral,
    VolumeIntegral,
    ClockContourIntegral,
    AntiClockContourIntegral,
}

public enum BracketSignType
{
    LeftRound, RightRound, LeftCurly, RightCurly, LeftSquare, RightSquare, LeftAngle, RightAngle,
    LeftBar, RightBar, LeftSquareBar, RightSquareBar, LeftDoubleBar, RightDoubleBar, LeftCeiling, RightCeiling,
    LeftFloor, RightFloor,
}

public enum HorizontalBracketSignType
{
    TopCurly, BottomCurly, TopSquare, BottomSquare,
}

public enum SubSuperType
{
    Sub, Super, SubAndSuper
}

public enum SignType
{
    Simple, Bottom, BottomTop, Sub, SubSuper
}

[EnumLocalize]
public enum EditorMode
{
    [EnumLocalizeKey(nameof(Localize.EditorMode_Math))]
    Math,

    [EnumLocalizeKey(nameof(Localize.EditorMode_Text))]
    Text,
}

[EnumLocalize]
public enum FontType
{
    [EnumLocalizeKey(nameof(Localize.FontType_SystemDefault))]
    SystemDefault,

    [EnumLocalizeValue("STIX")]
    STIXGeneral,

    [EnumLocalizeValue("STIX Integrals D")]
    STIXIntegralsD,

    [EnumLocalizeValue("STIX Integrals Sm")]
    STIXIntegralsSm,

    [EnumLocalizeValue("STIX Non Unicode")]
    STIXNonUnicode,

    [EnumLocalizeValue("STIX Size Three Sym")]
    STIXSizeThreeSym,

    [EnumLocalizeValue("STIX Size Two Sym")]
    STIXSizeTwoSym,

    [EnumLocalizeValue("STIX Variants")]
    STIXVariants,

    [EnumLocalizeValue("STIX Size Four Sym")]
    STIXSizeFourSym,

    [EnumLocalizeValue("STIX Integrals Up Sm")]
    STIXIntegralsUpSm,

    [EnumLocalizeValue("STIX Size One Sym")]
    STIXSizeOneSym,

    [EnumLocalizeValue("STIX Integrals Up D")]
    STIXIntegralsUpD,

    [EnumLocalizeValue("STIX Integrals Up")]
    STIXIntegralsUp,

    [EnumLocalizeValue("STIX Size Five Sym")]
    STIXSizeFiveSym,

    [EnumLocalizeValue("Segoe UI")]
    Segoe,

    [EnumLocalizeValue("Arial")]
    Arial,

    [EnumLocalizeValue("Times New Roman")]
    TimesNewRoman,

    [EnumLocalizeValue("Courier New")]
    CourierNew,

    [EnumLocalizeValue("Courier")]
    Courier,

    [EnumLocalizeValue("Georgia")]
    Georgia,

    [EnumLocalizeValue("Impact")]
    Impact,

    [EnumLocalizeValue("Lucida Sans Unicode")]
    LucidaSansUnicode,

    [EnumLocalizeValue("Tahoma")]
    Tahoma,

    [EnumLocalizeValue("Verdana")]
    Verdana,

    [EnumLocalizeValue("Webdings")]
    Webdings,

    [EnumLocalizeValue("Wingdings")]
    Wingdings,

    [EnumLocalizeValue("MS Serif")]
    MSSerif,

    [EnumLocalizeValue("MS Sans Serif")]
    MSSansSerif,

    [EnumLocalizeValue("Comic Sans MS")]
    ComicSansMS,

    [EnumLocalizeValue("Arial Black")]
    ArialBlack,

    [EnumLocalizeValue("Lucida Console")]
    LucidaConsole,

    [EnumLocalizeValue("Palatino Linotype")]
    PalatinoLinotype,

    [EnumLocalizeValue("Trebuchet MS")]
    TrebuchetMS,

    [EnumLocalizeValue("Symbol")]
    Symbol,

    //STIXGeneral,        STIXGeneralBol,     STIXGeneralBolIta,  STIXGeneralItalic,
    //STIXIntDBol,        STIXIntDReg,        STIXIntSmBol,       STIXIntSmReg,       STIXIntUpBol, STIXIntUpDBol,
    //STIXIntUpDReg,      STIXIntUpReg,       STIXIntUpSmBol,     STIXIntUpSmReg,     STIXNonUni,
    //STIXNonUniBol,      STIXNonUniBolIta,   STIXNonUniIta,      STIXSizFiveSymReg,  STIXSizFourSymBol,
    //STIXSizFourSymReg,  STIXSizOneSymBol,   STIXSizOneSymReg,   STIXSizThreeSymBol, STIXSizThreeSymReg,
    //STIXSizTwoSymBol,   STIXSizTwoSymReg,   STIXVar,            STIXVarBol,
}

[EnumLocalize]
public enum CopyType
{
    [EnumLocalizeKey(nameof(Localize.CopyType_Image))]
    Image,

    [EnumLocalizeKey(nameof(Localize.CopyType_Latex))]
    Latex,
}
