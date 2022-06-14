namespace RastDB.Exceptions;

public class DatabaseNotExistsException : Exception { }

public class DatabaseAlreadyExistsException : Exception { }
public class NotATableException : Exception { }

public class TableAlreadyExistsException : Exception { }

public class UnknownTypeException : Exception { }
public class IncorrectValueException : Exception { }

public class ValueNotPresentedException : Exception { }

public class TooManyArgumentsException : Exception { }

public class UnexpectedSituationException : Exception { }
