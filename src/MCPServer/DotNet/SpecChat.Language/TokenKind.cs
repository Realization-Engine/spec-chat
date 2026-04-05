namespace SpecChat.Language;

/// <summary>
/// Every token type recognized by the SpecChat lexer.
/// </summary>
public enum TokenKind
{
    // ── Literals ──────────────────────────────────────────────
    Integer,
    Float,
    String,
    Boolean,

    // ── Identifiers ──────────────────────────────────────────
    Ident,
    DottedIdent,

    // ── Data specification keywords ──────────────────────────
    KwEntity,
    KwEnum,
    KwInvariant,
    KwContract,
    KwRequires,
    KwEnsures,
    KwGuarantees,
    KwRefines,
    KwAs,
    KwRationale,
    KwContext,
    KwDecision,
    KwConsequence,
    KwSupersedes,

    // ── Expression keywords ──────────────────────────────────
    KwImplies,
    KwAnd,
    KwOr,
    KwNot,
    KwContains,
    KwExcludes,
    KwIn,
    KwAll,
    KwExists,
    KwHave,
    KwSatisfy,
    KwCount,

    // ── Systems specification keywords ───────────────────────
    KwSystem,
    KwTarget,
    KwResponsibility,
    KwAuthored,
    KwConsumed,
    KwComponent,
    KwKind,
    KwSource,
    KwVersion,
    KwUsedBy,
    KwTopology,
    KwAllow,
    KwDeny,
    KwPhase,
    KwProduces,
    KwGate,
    KwCommand,
    KwExpects,
    KwTrace,
    KwConstraint,
    KwScope,
    KwRule,
    KwPackagePolicy,
    KwCategory,
    KwIncludes,
    KwDefault,
    KwDotnet,
    KwSolution,
    KwFormat,
    KwStartup,
    KwFolder,
    KwProjects,
    KwPath,
    KwStatus,
    KwExisting,
    KwNew,

    // ── Design specification keywords ────────────────────────
    KwPage,
    KwHost,
    KwRoute,
    KwConcepts,
    KwRole,
    KwCrossLinks,
    KwVisualization,
    KwParameters,
    KwSliders,

    // ── Primitive type keywords ──────────────────────────────
    KwString,
    KwInt,
    KwDouble,
    KwBool,
    KwUnknown,

    // ── Symbols ──────────────────────────────────────────────
    LBrace,
    RBrace,
    LBracket,
    RBracket,
    LParen,
    RParen,
    Colon,
    Semicolon,
    Comma,
    Dot,
    Question,
    At,
    Arrow,
    DotDot,
    Eq,
    Neq,
    Lt,
    Gt,
    Lte,
    Gte,

    // ── End of input ─────────────────────────────────────────
    Eof,
}
