// Parse strings using a specification based on the Python format() syntax.
// 
//    ``parse()`` is the opposite of ``format()``
// 
// The module is set up to only export ``parse()``, ``search()``, ``findall()``,
// and ``with_pattern()`` when ``import \*`` is used:
// 
// >>> from parse import *
// 
// From there it's a simple thing to parse a string:
// 
// .. code-block:: pycon
// 
//     >>> parse("It's {}, I love it!", "It's spam, I love it!")
//     <Result ('spam',) {}>
//     >>> _[0]
//     'spam'
// 
// Or to search a string for some pattern:
// 
// .. code-block:: pycon
// 
//     >>> search('Age: {:d}\n', 'Name: Rufus\nAge: 42\nColor: red\n')
//     <Result (42,) {}>
// 
// Or find all the occurrences of some pattern in a string:
// 
// .. code-block:: pycon
// 
//     >>> ''.join(r[0] for r in findall(">{}<", "<p>the <b>bold</b> text</p>"))
//     'the bold text'
// 
// If you're going to use the same pattern to match lots of strings you can
// compile it once:
// 
// .. code-block:: pycon
// 
//     >>> from parse import compile
//     >>> p = compile("It's {}, I love it!")
//     >>> print(p)
//     <Parser "It's {}, I love it!">
//     >>> p.parse("It's spam, I love it!")
//     <Result ('spam',) {}>
// 
// ("compile" is not exported for ``import *`` usage as it would override the
// built-in ``compile()`` function)
// 
// The default behaviour is to match strings case insensitively. You may match with
// case by specifying `case_sensitive=True`:
// 
// .. code-block:: pycon
// 
//     >>> parse('SPAM', 'spam', case_sensitive=True) is None
//     True
// 
// 
// Format Syntax
// -------------
// 
// A basic version of the `Format String Syntax`_ is supported with anonymous
// (fixed-position), named and formatted fields::
// 
//    {[field name]:[format spec]}
// 
// Field names must be a valid Python identifiers, including dotted names;
// element indexes imply dictionaries (see below for example).
// 
// Numbered fields are also not supported: the result of parsing will include
// the parsed fields in the order they are parsed.
// 
// The conversion of fields to types other than strings is done based on the
// type in the format specification, which mirrors the ``format()`` behaviour.
// There are no "!" field conversions like ``format()`` has.
// 
// Some simple parse() format string examples:
// 
// .. code-block:: pycon
// 
//     >>> parse("Bring me a {}", "Bring me a shrubbery")
//     <Result ('shrubbery',) {}>
//     >>> r = parse("The {} who {} {}", "The knights who say Ni!")
//     >>> print(r)
//     <Result ('knights', 'say', 'Ni!') {}>
//     >>> print(r.fixed)
//     ('knights', 'say', 'Ni!')
//     >>> print(r[0])
//     knights
//     >>> print(r[1:])
//     ('say', 'Ni!')
//     >>> r = parse("Bring out the holy {item}", "Bring out the holy hand grenade")
//     >>> print(r)
//     <Result () {'item': 'hand grenade'}>
//     >>> print(r.named)
//     {'item': 'hand grenade'}
//     >>> print(r['item'])
//     hand grenade
//     >>> 'item' in r
//     True
// 
// Note that `in` only works if you have named fields.
// 
// Dotted names and indexes are possible with some limits. Only word identifiers
// are supported (ie. no numeric indexes) and the application must make additional
// sense of the result:
// 
// .. code-block:: pycon
// 
//     >>> r = parse("Mmm, {food.type}, I love it!", "Mmm, spam, I love it!")
//     >>> print(r)
//     <Result () {'food.type': 'spam'}>
//     >>> print(r.named)
//     {'food.type': 'spam'}
//     >>> print(r['food.type'])
//     spam
//     >>> r = parse("My quest is {quest[name]}", "My quest is to seek the holy grail!")
//     >>> print(r)
//     <Result () {'quest': {'name': 'to seek the holy grail!'}}>
//     >>> print(r['quest'])
//     {'name': 'to seek the holy grail!'}
//     >>> print(r['quest']['name'])
//     to seek the holy grail!
// 
// If the text you're matching has braces in it you can match those by including
// a double-brace ``{{`` or ``}}`` in your format string, just like format() does.
// 
// 
// Format Specification
// --------------------
// 
// Most often a straight format-less ``{}`` will suffice where a more complex
// format specification might have been used.
// 
// Most of `format()`'s `Format Specification Mini-Language`_ is supported:
// 
//    [[fill]align][0][width][.precision][type]
// 
// The differences between `parse()` and `format()` are:
// 
// - The align operators will cause spaces (or specified fill character) to be
//   stripped from the parsed value. The width is not enforced; it just indicates
//   there may be whitespace or "0"s to strip.
// - Numeric parsing will automatically handle a "0b", "0o" or "0x" prefix.
//   That is, the "#" format character is handled automatically by d, b, o
//   and x formats. For "d" any will be accepted, but for the others the correct
//   prefix must be present if at all.
// - Numeric sign is handled automatically.
// - The thousands separator is handled automatically if the "n" type is used.
// - The types supported are a slightly different mix to the format() types.  Some
//   format() types come directly over: "d", "n", "%", "f", "e", "b", "o" and "x".
//   In addition some regular expression character group types "D", "w", "W", "s"
//   and "S" are also available.
// - The "e" and "g" types are case-insensitive so there is not need for
//   the "E" or "G" types. The "e" type handles Fortran formatted numbers (no
//   leading 0 before the decimal point).
// 
// ===== =========================================== ========
// Type  Characters Matched                          Output
// ===== =========================================== ========
// l     Letters (ASCII)                             str
// w     Letters, numbers and underscore             str
// W     Not letters, numbers and underscore         str
// s     Whitespace                                  str
// S     Non-whitespace                              str
// d     Digits (effectively integer numbers)        int
// D     Non-digit                                   str
// n     Numbers with thousands separators (, or .)  int
// %     Percentage (converted to value/100.0)       float
// f     Fixed-point numbers                         float
// F     Decimal numbers                             Decimal
// e     Floating-point numbers with exponent        float
//       e.g. 1.1e-10, NAN (all case insensitive)
// g     General number format (either d, f or e)    float
// b     Binary numbers                              int
// o     Octal numbers                               int
// x     Hexadecimal numbers (lower and upper case)  int
// ti    ISO 8601 format date/time                   datetime
//       e.g. 1972-01-20T10:21:36Z ("T" and "Z"
//       optional)
// te    RFC2822 e-mail format date/time             datetime
//       e.g. Mon, 20 Jan 1972 10:21:36 +1000
// tg    Global (day/month) format date/time         datetime
//       e.g. 20/1/1972 10:21:36 AM +1:00
// ta    US (month/day) format date/time             datetime
//       e.g. 1/20/1972 10:21:36 PM +10:30
// tc    ctime() format date/time                    datetime
//       e.g. Sun Sep 16 01:03:52 1973
// th    HTTP log format date/time                   datetime
//       e.g. 21/Nov/2011:00:07:11 +0000
// ts    Linux system log format date/time           datetime
//       e.g. Nov  9 03:37:44
// tt    Time                                        time
//       e.g. 10:21:36 PM -5:30
// ===== =========================================== ========
// 
// Some examples of typed parsing with ``None`` returned if the typing
// does not match:
// 
// .. code-block:: pycon
// 
//     >>> parse('Our {:d} {:w} are...', 'Our 3 weapons are...')
//     <Result (3, 'weapons') {}>
//     >>> parse('Our {:d} {:w} are...', 'Our three weapons are...')
//     >>> parse('Meet at {:tg}', 'Meet at 1/2/2011 11:00 PM')
//     <Result (datetime.datetime(2011, 2, 1, 23, 0),) {}>
// 
// And messing about with alignment:
// 
// .. code-block:: pycon
// 
//     >>> parse('with {:>} herring', 'with     a herring')
//     <Result ('a',) {}>
//     >>> parse('spam {:^} spam', 'spam    lovely     spam')
//     <Result ('lovely',) {}>
// 
// Note that the "center" alignment does not test to make sure the value is
// centered - it just strips leading and trailing whitespace.
// 
// Width and precision may be used to restrict the size of matched text
// from the input. Width specifies a minimum size and precision specifies
// a maximum. For example:
// 
// .. code-block:: pycon
// 
//     >>> parse('{:.2}{:.2}', 'look')           # specifying precision
//     <Result ('lo', 'ok') {}>
//     >>> parse('{:4}{:4}', 'look at that')     # specifying width
//     <Result ('look', 'at that') {}>
//     >>> parse('{:4}{:.4}', 'look at that')    # specifying both
//     <Result ('look at ', 'that') {}>
//     >>> parse('{:2d}{:2d}', '0440')           # parsing two contiguous numbers
//     <Result (4, 40) {}>
// 
// Some notes for the date and time types:
// 
// - the presence of the time part is optional (including ISO 8601, starting
//   at the "T"). A full datetime object will always be returned; the time
//   will be set to 00:00:00. You may also specify a time without seconds.
// - when a seconds amount is present in the input fractions will be parsed
//   to give microseconds.
// - except in ISO 8601 the day and month digits may be 0-padded.
// - the date separator for the tg and ta formats may be "-" or "/".
// - named months (abbreviations or full names) may be used in the ta and tg
//   formats in place of numeric months.
// - as per RFC 2822 the e-mail format may omit the day (and comma), and the
//   seconds but nothing else.
// - hours greater than 12 will be happily accepted.
// - the AM/PM are optional, and if PM is found then 12 hours will be added
//   to the datetime object's hours amount - even if the hour is greater
//   than 12 (for consistency.)
// - in ISO 8601 the "Z" (UTC) timezone part may be a numeric offset
// - timezones are specified as "+HH:MM" or "-HH:MM". The hour may be one or two
//   digits (0-padded is OK.) Also, the ":" is optional.
// - the timezone is optional in all except the e-mail format (it defaults to
//   UTC.)
// - named timezones are not handled yet.
// 
// Note: attempting to match too many datetime fields in a single parse() will
// currently result in a resource allocation issue. A TooManyFields exception
// will be raised in this instance. The current limit is about 15. It is hoped
// that this limit will be removed one day.
// 
// .. _`Format String Syntax`:
//   http://docs.python.org/library/string.html#format-string-syntax
// .. _`Format Specification Mini-Language`:
//   http://docs.python.org/library/string.html#format-specification-mini-language
// 
// 
// Result and Match Objects
// ------------------------
// 
// The result of a ``parse()`` and ``search()`` operation is either ``None`` (no match), a
// ``Result`` instance or a ``Match`` instance if ``evaluate_result`` is False.
// 
// The ``Result`` instance has three attributes:
// 
// ``fixed``
//    A tuple of the fixed-position, anonymous fields extracted from the input.
// ``named``
//    A dictionary of the named fields extracted from the input.
// ``spans``
//    A dictionary mapping the names and fixed position indices matched to a
//    2-tuple slice range of where the match occurred in the input.
//    The span does not include any stripped padding (alignment or width).
// 
// The ``Match`` instance has one method:
// 
// ``evaluate_result()``
//    Generates and returns a ``Result`` instance for this ``Match`` object.
// 
// 
// 
// Custom Type Conversions
// -----------------------
// 
// If you wish to have matched fields automatically converted to your own type you
// may pass in a dictionary of type conversion information to ``parse()`` and
// ``compile()``.
// 
// The converter will be passed the field string matched. Whatever it returns
// will be substituted in the ``Result`` instance for that field.
// 
// Your custom type conversions may override the builtin types if you supply one
// with the same identifier:
// 
// .. code-block:: pycon
// 
//     >>> def shouty(string):
//     ...    return string.upper()
//     ...
//     >>> parse('{:shouty} world', 'hello world', dict(shouty=shouty))
//     <Result ('HELLO',) {}>
// 
// If the type converter has the optional ``pattern`` attribute, it is used as
// regular expression for better pattern matching (instead of the default one):
// 
// .. code-block:: pycon
// 
//     >>> def parse_number(text):
//     ...    return int(text)
//     >>> parse_number.pattern = r'\d+'
//     >>> parse('Answer: {number:Number}', 'Answer: 42', dict(Number=parse_number))
//     <Result () {'number': 42}>
//     >>> _ = parse('Answer: {:Number}', 'Answer: Alice', dict(Number=parse_number))
//     >>> assert _ is None, "MISMATCH"
// 
// You can also use the ``with_pattern(pattern)`` decorator to add this
// information to a type converter function:
// 
// .. code-block:: pycon
// 
//     >>> from parse import with_pattern
//     >>> @with_pattern(r'\d+')
//     ... def parse_number(text):
//     ...    return int(text)
//     >>> parse('Answer: {number:Number}', 'Answer: 42', dict(Number=parse_number))
//     <Result () {'number': 42}>
// 
// A more complete example of a custom type might be:
// 
// .. code-block:: pycon
// 
//     >>> yesno_mapping = {
//     ...     "yes":  True,   "no":    False,
//     ...     "on":   True,   "off":   False,
//     ...     "true": True,   "false": False,
//     ... }
//     >>> @with_pattern(r"|".join(yesno_mapping))
//     ... def parse_yesno(text):
//     ...     return yesno_mapping[text.lower()]
// 
// 
// If the type converter ``pattern`` uses regex-grouping (with parenthesis),
// you should indicate this by using the optional ``regex_group_count`` parameter
// in the ``with_pattern()`` decorator:
// 
// .. code-block:: pycon
// 
//     >>> @with_pattern(r'((\d+))', regex_group_count=2)
//     ... def parse_number2(text):
//     ...    return int(text)
//     >>> parse('Answer: {:Number2} {:Number2}', 'Answer: 42 43', dict(Number2=parse_number2))
//     <Result (42, 43) {}>
// 
// Otherwise, this may cause parsing problems with unnamed/fixed parameters.
// 
// 
// Potential Gotchas
// -----------------
// 
// ``parse()`` will always match the shortest text necessary (from left to right)
// to fulfil the parse pattern, so for example:
// 
// 
// .. code-block:: pycon
// 
//     >>> pattern = '{dir1}/{dir2}'
//     >>> data = 'root/parent/subdir'
//     >>> sorted(parse(pattern, data).named.items())
//     [('dir1', 'root'), ('dir2', 'parent/subdir')]
// 
// So, even though `{'dir1': 'root/parent', 'dir2': 'subdir'}` would also fit
// the pattern, the actual match represents the shortest successful match for
// ``dir1``.
// 
// ----
// 
// - 1.19.0 Added slice access to fixed results (thanks @jonathangjertsen).
//   Also corrected matching of *full string* vs. *full line* (thanks @giladreti)
//   Fix issue with using digit field numbering and types
// - 1.18.0 Correct bug in int parsing introduced in 1.16.0 (thanks @maxxk)
// - 1.17.0 Make left- and center-aligned search consume up to next space
// - 1.16.0 Make compiled parse objects pickleable (thanks @martinResearch)
// - 1.15.0 Several fixes for parsing non-base 10 numbers (thanks @vladikcomper)
// - 1.14.0 More broad acceptance of Fortran number format (thanks @purpleskyfall)
// - 1.13.1 Project metadata correction.
// - 1.13.0 Handle Fortran formatted numbers with no leading 0 before decimal
//   point (thanks @purpleskyfall).
//   Handle comparison of FixedTzOffset with other types of object.
// - 1.12.1 Actually use the `case_sensitive` arg in compile (thanks @jacquev6)
// - 1.12.0 Do not assume closing brace when an opening one is found (thanks @mattsep)
// - 1.11.1 Revert having unicode char in docstring, it breaks Bamboo builds(?!)
// - 1.11.0 Implement `__contains__` for Result instances.
// - 1.10.0 Introduce a "letters" matcher, since "w" matches numbers
//   also.
// - 1.9.1 Fix deprecation warnings around backslashes in regex strings
//   (thanks Mickael Schoentgen). Also fix some documentation formatting
//   issues.
// - 1.9.0 We now honor precision and width specifiers when parsing numbers
//   and strings, allowing parsing of concatenated elements of fixed width
//   (thanks Julia Signell)
// - 1.8.4 Add LICENSE file at request of packagers.
//   Correct handling of AM/PM to follow most common interpretation.
//   Correct parsing of hexadecimal that looks like a binary prefix.
//   Add ability to parse case sensitively.
//   Add parsing of numbers to Decimal with "F" (thanks John Vandenberg)
// - 1.8.3 Add regex_group_count to with_pattern() decorator to support
//   user-defined types that contain brackets/parenthesis (thanks Jens Engel)
// - 1.8.2 add documentation for including braces in format string
// - 1.8.1 ensure bare hexadecimal digits are not matched
// - 1.8.0 support manual control over result evaluation (thanks Timo Furrer)
// - 1.7.0 parse dict fields (thanks Mark Visser) and adapted to allow
//   more than 100 re groups in Python 3.5+ (thanks David King)
// - 1.6.6 parse Linux system log dates (thanks Alex Cowan)
// - 1.6.5 handle precision in float format (thanks Levi Kilcher)
// - 1.6.4 handle pipe "|" characters in parse string (thanks Martijn Pieters)
// - 1.6.3 handle repeated instances of named fields, fix bug in PM time
//   overflow
// - 1.6.2 fix logging to use local, not root logger (thanks Necku)
// - 1.6.1 be more flexible regarding matched ISO datetimes and timezones in
//   general, fix bug in timezones without ":" and improve docs
// - 1.6.0 add support for optional ``pattern`` attribute in user-defined types
//   (thanks Jens Engel)
// - 1.5.3 fix handling of question marks
// - 1.5.2 fix type conversion error with dotted names (thanks Sebastian Thiel)
// - 1.5.1 implement handling of named datetime fields
// - 1.5 add handling of dotted field names (thanks Sebastian Thiel)
// - 1.4.1 fix parsing of "0" in int conversion (thanks James Rowe)
// - 1.4 add __getitem__ convenience access on Result.
// - 1.3.3 fix Python 2.5 setup.py issue.
// - 1.3.2 fix Python 3.2 setup.py issue.
// - 1.3.1 fix a couple of Python 3.2 compatibility issues.
// - 1.3 added search() and findall(); removed compile() from ``import *``
//   export as it overwrites builtin.
// - 1.2 added ability for custom and override type conversions to be
//   provided; some cleanup
// - 1.1.9 to keep things simpler number sign is handled automatically;
//   significant robustification in the face of edge-case input.
// - 1.1.8 allow "d" fields to have number base "0x" etc. prefixes;
//   fix up some field type interactions after stress-testing the parser;
//   implement "%" type.
// - 1.1.7 Python 3 compatibility tweaks (2.5 to 2.7 and 3.2 are supported).
// - 1.1.6 add "e" and "g" field types; removed redundant "h" and "X";
//   removed need for explicit "#".
// - 1.1.5 accept textual dates in more places; Result now holds match span
//   positions.
// - 1.1.4 fixes to some int type conversion; implemented "=" alignment; added
//   date/time parsing with a variety of formats handled.
// - 1.1.3 type conversion is automatic based on specified field types. Also added
//   "f" and "n" types.
// - 1.1.2 refactored, added compile() and limited ``from parse import *``
// - 1.1.1 documentation improvements
// - 1.1.0 implemented more of the `Format Specification Mini-Language`_
//   and removed the restriction on mixing fixed-position and named fields
// - 1.0.0 initial release
// 
// This code is copyright 2012-2021 Richard Jones <richard@python.org>
// See the end of the source file for the license of use.
// 
namespace Namespace {
    
    using @absolute_import = @@__future__.absolute_import;
    
    using re;
    
    using sys;
    
    using datetime = datetime.datetime;
    
    using time = datetime.time;
    
    using tzinfo = datetime.tzinfo;
    
    using timedelta = datetime.timedelta;
    
    using Decimal = @decimal.Decimal;
    
    using partial = functools.partial;
    
    using logging;
    
    using System;
    
    using System.Collections.Generic;
    
    using System.Linq;
    
    public static class Module {
        
        public static object @__version__ = "1.19.0";
        
        public static object @__all__ = "parse search findall with_pattern".split();
        
        public static object log = logging.getLogger(@__name__);
        
        // Attach a regular expression pattern matcher to a custom type converter
        //     function.
        // 
        //     This annotates the type converter with the :attr:`pattern` attribute.
        // 
        //     EXAMPLE:
        //         >>> import parse
        //         >>> @parse.with_pattern(r"\d+")
        //         ... def parse_number(text):
        //         ...     return int(text)
        // 
        //     is equivalent to:
        // 
        //         >>> def parse_number(text):
        //         ...     return int(text)
        //         >>> parse_number.pattern = r"\d+"
        // 
        //     :param pattern: regular expression pattern (as text)
        //     :param regex_group_count: Indicates how many regex-groups are in pattern.
        //     :return: wrapped function
        //     
        public static object with_pattern(object pattern, object regex_group_count = null) {
            Func<object, object> decorator = func => {
                func.pattern = pattern;
                func.regex_group_count = regex_group_count;
                return func;
            };
            return decorator;
        }
        
        // Convert a string to an integer.
        // 
        //     The string may start with a sign.
        // 
        //     It may be of a base other than 2, 8, 10 or 16.
        // 
        //     If base isn't specified, it will be detected automatically based
        //     on a string format. When string starts with a base indicator, 0#nnnn,
        //     it overrides the default base of 10.
        // 
        //     It may also have other non-numeric characters that we can ignore.
        //     
        public class int_convert {
            
            public object CHARS = "0123456789abcdefghijklmnopqrstuvwxyz";
            
            public int_convert(object @base = null) {
                this.@base = @base;
            }
            
            public virtual object @__call__(object @string, object match) {
                if (@string[0] == "-") {
                    var sign = -1;
                    var number_start = 1;
                } else if (@string[0] == "+") {
                    sign = 1;
                    number_start = 1;
                } else {
                    sign = 1;
                    number_start = 0;
                }
                var @base = this.@base;
                // If base wasn't specified, detect it automatically
                if (@base == null) {
                    // Assume decimal number, unless different base is detected
                    @base = 10;
                    // For number formats starting with 0b, 0o, 0x, use corresponding base ...
                    if (@string[number_start] == "0" && @string.Count - number_start > 2) {
                        if ("bB".Contains(@string[number_start + 1])) {
                            @base = 2;
                        } else if ("oO".Contains(@string[number_start + 1])) {
                            @base = 8;
                        } else if ("xX".Contains(@string[number_start + 1])) {
                            @base = 16;
                        }
                    }
                }
                var chars = int_convert.CHARS[::base];
                @string = re.sub(String.Format("[^%s]", chars), "", @string.lower());
                return sign * Convert.ToInt32(@string, @base);
            }
        }
        
        // Convert the first element of a pair.
        //     This equivalent to lambda s,m: converter(s). But unlike a lambda function, it can be pickled
        //     
        public class convert_first {
            
            public convert_first(object converter) {
                this.converter = converter;
            }
            
            public virtual object @__call__(object @string, object match) {
                return this.converter(@string);
            }
        }
        
        public static object percentage(object @string, object match) {
            return float(@string[:: - 1]) / 100.0;
        }
        
        // Fixed offset in minutes east from UTC.
        public class FixedTzOffset
            : tzinfo {
            
            public object ZERO = timedelta(0);
            
            public FixedTzOffset(object offset, object name) {
                this._offset = timedelta(minutes: offset);
                this._name = name;
            }
            
            public virtual object @__repr__() {
                return String.Format("<%s %s %s>", this.@__class__.@__name__, this._name, this._offset);
            }
            
            public virtual object utcoffset(object dt) {
                return this._offset;
            }
            
            public virtual object tzname(object dt) {
                return this._name;
            }
            
            public virtual object dst(object dt) {
                return this.ZERO;
            }
            
            public virtual object @__eq__(object other) {
                if (!(other is FixedTzOffset)) {
                    return false;
                }
                return this._name == other._name && this._offset == other._offset;
            }
        }
        
        public static object MONTHS_MAP = new Dictionary<@string, object> {
            {
                "Jan",
                1},
            {
                "January",
                1},
            {
                "Feb",
                2},
            {
                "February",
                2},
            {
                "Mar",
                3},
            {
                "March",
                3},
            {
                "Apr",
                4},
            {
                "April",
                4},
            {
                "May",
                5},
            {
                "Jun",
                6},
            {
                "June",
                6},
            {
                "Jul",
                7},
            {
                "July",
                7},
            {
                "Aug",
                8},
            {
                "August",
                8},
            {
                "Sep",
                9},
            {
                "September",
                9},
            {
                "Oct",
                10},
            {
                "October",
                10},
            {
                "Nov",
                11},
            {
                "November",
                11},
            {
                "Dec",
                12},
            {
                "December",
                12}};
        
        public static object DAYS_PAT = @"(Mon|Tue|Wed|Thu|Fri|Sat|Sun)";
        
        public static object MONTHS_PAT = @"(Jan|Feb|Mar|Apr|May|Jun|Jul|Aug|Sep|Oct|Nov|Dec)";
        
        public static object ALL_MONTHS_PAT = String.Format(@"(%s)", "|".join(MONTHS_MAP));
        
        public static object TIME_PAT = @"(\d{1,2}:\d{1,2}(:\d{1,2}(\.\d+)?)?)";
        
        public static object AM_PAT = @"(\s+[AP]M)";
        
        public static object TZ_PAT = @"(\s+[-+]\d\d?:?\d\d)";
        
        // Convert the incoming string containing some date / time info into a
        //     datetime instance.
        //     
        public static object date_convert(
            object @string,
            object match,
            object ymd = null,
            object mdy = null,
            object dmy = null,
            object d_m_y = null,
            object hms = null,
            object am = null,
            object tz = null,
            object mm = null,
            object dd = null) {
            object M;
            object d;
            object m;
            object y;
            var groups = match.groups();
            var time_only = false;
            if (mm && dd) {
                y = datetime.today().year;
                m = groups[mm];
                d = groups[dd];
            } else if (ymd != null) {
                var _tup_1 = re.split(@"[-/\s]", groups[ymd]);
                y = _tup_1.Item1;
                m = _tup_1.Item2;
                d = _tup_1.Item3;
            } else if (mdy != null) {
                var _tup_2 = re.split(@"[-/\s]", groups[mdy]);
                m = _tup_2.Item1;
                d = _tup_2.Item2;
                y = _tup_2.Item3;
            } else if (dmy != null) {
                var _tup_3 = re.split(@"[-/\s]", groups[dmy]);
                d = _tup_3.Item1;
                m = _tup_3.Item2;
                y = _tup_3.Item3;
            } else if (d_m_y != null) {
                var _tup_4 = d_m_y;
                d = _tup_4.Item1;
                m = _tup_4.Item2;
                y = _tup_4.Item3;
                d = groups[d];
                m = groups[m];
                y = groups[y];
            } else {
                time_only = true;
            }
            var H = 0;
            if (hms != null && groups[hms]) {
                var t = groups[hms].split(":");
                if (t.Count == 2) {
                    var _tup_5 = t;
                    H = _tup_5.Item1;
                    M = _tup_5.Item2;
                } else {
                    var _tup_6 = t;
                    H = _tup_6.Item1;
                    M = _tup_6.Item2;
                    var S = _tup_6.Item3;
                    if (S.Contains(".")) {
                        var _tup_7 = S.split(".");
                        S = _tup_7.Item1;
                        var u = _tup_7.Item2;
                        u = Convert.ToInt32(float("." + u) * 1000000);
                    }
                    S = Convert.ToInt32(S);
                }
                H = Convert.ToInt32(H);
                M = Convert.ToInt32(M);
            }
            if (am != null) {
                am = groups[am];
                if (am) {
                    am = am.strip();
                }
                if (am == "AM" && H == 12) {
                    // correction for "12" hour functioning as "0" hour: 12:15 AM = 00:15 by 24 hr clock
                    H -= 12;
                } else if (am == "PM" && H == 12) {
                    // no correction needed: 12PM is midday, 12:00 by 24 hour clock
                } else if (am == "PM") {
                    H += 12;
                }
            }
            if (tz != null) {
                tz = groups[tz];
            }
            if (tz == "Z") {
                tz = FixedTzOffset(0, "UTC");
            } else if (tz) {
                tz = tz.strip();
                if (tz.isupper()) {
                    // TODO use the awesome python TZ module?
                } else {
                    var sign = tz[0];
                    if (tz.Contains(":")) {
                        var _tup_8 = tz[1].split(":");
                        var tzh = _tup_8.Item1;
                        var tzm = _tup_8.Item2;
                    } else if (tz.Count == 4) {
                        // 'snnn'
                        tzh = tz[1];
                        tzm = tz[2::4];
                    } else {
                        tzh = tz[1::3];
                        tzm = tz[3::5];
                    }
                    var offset = Convert.ToInt32(tzm) + Convert.ToInt32(tzh) * 60;
                    if (sign == "-") {
                        offset = -offset;
                    }
                    tz = FixedTzOffset(offset, tz);
                }
            }
            if (time_only) {
                d = time(H, M, S, u, tzinfo: tz);
            } else {
                y = Convert.ToInt32(y);
                if (m.isdigit()) {
                    m = Convert.ToInt32(m);
                } else {
                    m = MONTHS_MAP[m];
                }
                d = Convert.ToInt32(d);
                d = datetime(y, m, d, H, M, S, u, tzinfo: tz);
            }
            return d;
        }
        
        public class TooManyFields
            : ValueError {
        }
        
        public class RepeatedNameError
            : ValueError {
        }
        
        public static object REGEX_SAFETY = re.compile(@"([?\\\\.[\]()*+\^$!\|])");
        
        public static object ALLOWED_TYPES = new HashSet<object>("nbox%fFegwWdDsSl".ToList() + (from c in "ieahgcts"
            select ("t" + c)).ToList());
        
        // Pull apart the format [[fill]align][sign][0][width][.precision][type]
        public static object extract_format(object format, object extra_types) {
            object align;
            object fill = null;
            if ("<>=^".Contains(format[0])) {
                align = format[0];
                format = format[1];
            } else if (format.Count > 1 && "<>=^".Contains(format[1])) {
                fill = format[0];
                align = format[1];
                format = format[2];
            }
            if (format.startswith(("+", "-", " "))) {
                format = format[1];
            }
            var zero = false;
            if (format && format[0] == "0") {
                zero = true;
                format = format[1];
            }
            var width = "";
            while (format) {
                if (!format[0].isdigit()) {
                    break;
                }
                width += format[0];
                format = format[1];
            }
            if (format.startswith(".")) {
                // Precision isn't needed but we need to capture it so that
                // the ValueError isn't raised.
                format = format[1];
                var precision = "";
                while (format) {
                    if (!format[0].isdigit()) {
                        break;
                    }
                    precision += format[0];
                    format = format[1];
                }
            }
            // the rest is the type, if present
            var type = format;
            if (type && !ALLOWED_TYPES.Contains(type) && !extra_types.Contains(type)) {
                throw ValueError(String.Format("format spec %r not recognised", type));
            }
            return locals();
        }
        
        public static object PARSE_RE = re.compile(@"({{|}}|{\w*(?:(?:\.\w+)|(?:\[[^\]]+\]))*(?::[^}]+)?})");
        
        // Encapsulate a format string that may be used to parse other strings.
        public class Parser
            : object {
            
            public Parser(object format, object extra_types = null, object case_sensitive = false) {
                // a mapping of a name as in {hello.world} to a regex-group compatible
                // name, like hello__world Its used to prevent the transformation of
                // name-to-group and group to name to fail subtly, such as in:
                // hello_.world-> hello___world->hello._world
                this._group_to_name_map = new Dictionary<object, object> {
                };
                // also store the original field name to group name mapping to allow
                // multiple instances of a name in the format string
                this._name_to_group_map = new Dictionary<object, object> {
                };
                // and to sanity check the repeated instances store away the first
                // field type specification for the named field
                this._name_types = new Dictionary<object, object> {
                };
                this._format = format;
                if (extra_types == null) {
                    extra_types = new Dictionary<object, object> {
                    };
                }
                this._extra_types = extra_types;
                if (case_sensitive) {
                    this._re_flags = re.DOTALL;
                } else {
                    this._re_flags = re.IGNORECASE | re.DOTALL;
                }
                this._fixed_fields = new List<object>();
                this._named_fields = new List<object>();
                this._group_index = 0;
                this._type_conversions = new Dictionary<object, object> {
                };
                this._expression = this._generate_expression();
                this.@__search_re = null;
                this.@__match_re = null;
                log.debug("format %r -> %r", format, this._expression);
            }
            
            public virtual object @__repr__() {
                if (this._format.Count > 20) {
                    return String.Format("<%s %r>", this.@__class__.@__name__, this._format[::17] + "...");
                }
                return String.Format("<%s %r>", this.@__class__.@__name__, this._format);
            }
            
            public object _search_re {
                get {
                    if (this.@__search_re == null) {
                        try {
                            this.@__search_re = re.compile(this._expression, this._re_flags);
                        } catch (AssertionError) {
                            // access error through sys to keep py3k and backward compat
                            var e = sys.exc_info()[1].ToString();
                            if (e.endswith("this version only supports 100 named groups")) {
                                throw TooManyFields("sorry, you are attempting to parse too many complex fields");
                            }
                        }
                    }
                    return this.@__search_re;
                }
            }
            
            public object _match_re {
                get {
                    if (this.@__match_re == null) {
                        var expression = String.Format(@"\A%s\Z", this._expression);
                        try {
                            this.@__match_re = re.compile(expression, this._re_flags);
                        } catch (AssertionError) {
                            // access error through sys to keep py3k and backward compat
                            var e = sys.exc_info()[1].ToString();
                            if (e.endswith("this version only supports 100 named groups")) {
                                throw TooManyFields("sorry, you are attempting to parse too many complex fields");
                            }
                        } catch {
                            throw NotImplementedError(String.Format("Group names (e.g. (?P<name>) can cause failure, as they are not escaped properly: '%s'", expression));
                        }
                    }
                    return this.@__match_re;
                }
            }
            
            public object named_fields {
                get {
                    return this._named_fields.copy();
                }
            }
            
            public object fixed_fields {
                get {
                    return this._fixed_fields.copy();
                }
            }
            
            // Match my format to the string exactly.
            // 
            //         Return a Result or Match instance or None if there's no match.
            //         
            public virtual object parse(object @string, object evaluate_result = true) {
                var m = this._match_re.match(@string);
                if (m == null) {
                    return null;
                }
                if (evaluate_result) {
                    return this.evaluate_result(m);
                } else {
                    return Match(this, m);
                }
            }
            
            // Search the string for my format.
            // 
            //         Optionally start the search at "pos" character index and limit the
            //         search to a maximum index of endpos - equivalent to
            //         search(string[:endpos]).
            // 
            //         If the ``evaluate_result`` argument is set to ``False`` a
            //         Match instance is returned instead of the actual Result instance.
            // 
            //         Return either a Result instance or None if there's no match.
            //         
            public virtual object search(object @string, object pos = 0, object endpos = null, object evaluate_result = true) {
                if (endpos == null) {
                    endpos = @string.Count;
                }
                var m = this._search_re.search(@string, pos, endpos);
                if (m == null) {
                    return null;
                }
                if (evaluate_result) {
                    return this.evaluate_result(m);
                } else {
                    return Match(this, m);
                }
            }
            
            // Search "string" for all occurrences of "format".
            // 
            //         Optionally start the search at "pos" character index and limit the
            //         search to a maximum index of endpos - equivalent to
            //         search(string[:endpos]).
            // 
            //         Returns an iterator that holds Result or Match instances for each format match
            //         found.
            //         
            public virtual object findall(
                object @string,
                object pos = 0,
                object endpos = null,
                object extra_types = null,
                object evaluate_result = true) {
                if (endpos == null) {
                    endpos = @string.Count;
                }
                return ResultIterator(this, @string, pos, endpos, evaluate_result: evaluate_result);
            }
            
            public virtual object _expand_named_fields(object named_fields) {
                var result = new Dictionary<object, object> {
                };
                foreach (var _tup_1 in named_fields.items()) {
                    var field = _tup_1.Item1;
                    var value = _tup_1.Item2;
                    // split 'aaa[bbb][ccc]...' into 'aaa' and '[bbb][ccc]...'
                    var _tup_2 = re.match(@"([^\[]+)(.*)", field).groups();
                    var basename = _tup_2.Item1;
                    var subkeys = _tup_2.Item2;
                    // create nested dictionaries {'aaa': {'bbb': {'ccc': ...}}}
                    var d = result;
                    var k = basename;
                    if (subkeys) {
                        foreach (var subkey in re.findall(@"\[[^\]]+\]", subkeys)) {
                            d = d.setdefault(k, new Dictionary<object, object> {
                            });
                            k = subkey[1:: - 1];
                        }
                    }
                    // assign the value to the last key
                    d[k] = value;
                }
                return result;
            }
            
            // Generate a Result instance for the given regex match object
            public virtual object evaluate_result(object m) {
                object value;
                // ok, figure the fixed fields we've pulled out and type convert them
                var fixed_fields = m.groups().ToList();
                foreach (var n in this._fixed_fields) {
                    if (this._type_conversions.Contains(n)) {
                        fixed_fields[n] = this._type_conversions[n](fixed_fields[n], m);
                    }
                }
                fixed_fields = tuple(from n in this._fixed_fields
                    select fixed_fields[n]);
                // grab the named fields, converting where requested
                var groupdict = m.groupdict();
                var named_fields = new Dictionary<object, object> {
                };
                var name_map = new Dictionary<object, object> {
                };
                foreach (var k in this._named_fields) {
                    var korig = this._group_to_name_map[k];
                    name_map[korig] = k;
                    if (this._type_conversions.Contains(k)) {
                        value = this._type_conversions[k](groupdict[k], m);
                    } else {
                        value = groupdict[k];
                    }
                    named_fields[korig] = value;
                }
                // now figure the match spans
                var spans = (from n in named_fields
                    select (n, m.span(name_map[n]))).ToDictionary();
                spans.update(from _tup_1 in this._fixed_fields.Select((_p_1,_p_2) => Tuple.Create(_p_2, _p_1)).Chop((i,n) => (i, n))
                    let i = _tup_1.Item1
                    let n = _tup_1.Item2
                    select (i, m.span(n + 1)));
                // and that's our result
                return Result(fixed_fields, this._expand_named_fields(named_fields), spans);
            }
            
            public virtual object _regex_replace(object match) {
                return "\\" + match.group(1);
            }
            
            public virtual object _generate_expression() {
                // turn my _format attribute into the _expression attribute
                var e = new List<object>();
                foreach (var part in PARSE_RE.split(this._format)) {
                    if (!part) {
                        continue;
                    } else if (part == "{{") {
                        e.append(@"\{");
                    } else if (part == "}}") {
                        e.append(@"\}");
                    } else if (part[0] == "{" && part[-1] == "}") {
                        // this will be a braces-delimited field to handle
                        e.append(this._handle_field(part));
                    } else {
                        // just some text to match
                        e.append(REGEX_SAFETY.sub(this._regex_replace, part));
                    }
                }
                return "".join(e);
            }
            
            public virtual object _to_group_name(object field) {
                // return a version of field which can be used as capture group, even
                // though it might contain '.'
                var group = field.replace(".", "_").replace("[", "_").replace("]", "_");
                // make sure we don't collide ("a.b" colliding with "a_b")
                var n = 1;
                while (this._group_to_name_map.Contains(group)) {
                    n += 1;
                    if (field.Contains(".")) {
                        group = field.replace(".", "_" * n);
                    } else if (field.Contains("_")) {
                        group = field.replace("_", "_" * n);
                    } else {
                        throw KeyError(String.Format("duplicated group name %r", field));
                    }
                }
                // save off the mapping
                this._group_to_name_map[group] = field;
                this._name_to_group_map[field] = group;
                return group;
            }
            
            public virtual object _handle_field(object field) {
                object n;
                object width;
                object s;
                object wrap;
                object group;
                object name;
                // first: lose the braces
                field = field[1:: - 1];
                // now figure whether this is an anonymous or named field, and whether
                // there's any format specification
                var format = "";
                if (field.Contains(":")) {
                    var _tup_1 = field.split(":");
                    name = _tup_1.Item1;
                    format = _tup_1.Item2;
                } else {
                    name = field;
                }
                // This *should* be more flexible, but parsing complicated structures
                // out of the string is hard (and not necessarily useful) ... and I'm
                // being lazy. So for now `identifier` is "anything starting with a
                // letter" and digit args don't get attribute or element stuff.
                if (name && name[0].isalpha()) {
                    if (this._name_to_group_map.Contains(name)) {
                        if (this._name_types[name] != format) {
                            throw RepeatedNameError(String.Format("field type %r for field \"%s\" does not match previous seen type %r", format, name, this._name_types[name]));
                        }
                        group = this._name_to_group_map[name];
                        // match previously-seen value
                        return String.Format(@"(?P=%s)", group);
                    } else {
                        group = this._to_group_name(name);
                        this._name_types[name] = format;
                    }
                    this._named_fields.append(group);
                    // this will become a group, which must not contain dots
                    wrap = String.Format(@"(?P<%s>%%s)", group);
                } else {
                    this._fixed_fields.append(this._group_index);
                    wrap = @"(%s)";
                    group = this._group_index;
                }
                // simplest case: no type specifier ({} or {name})
                if (!format) {
                    this._group_index += 1;
                    return wrap % @".+?";
                }
                // decode the format specification
                format = extract_format(format, this._extra_types);
                // figure type conversions, if any
                var type = format["type"];
                var is_numeric = type && "n%fegdobx".Contains(type);
                if (this._extra_types.Contains(type)) {
                    var type_converter = this._extra_types[type];
                    s = getattr(type_converter, "pattern", @".+?");
                    var regex_group_count = getattr(type_converter, "regex_group_count", 0);
                    if (regex_group_count == null) {
                        regex_group_count = 0;
                    }
                    this._group_index += regex_group_count;
                    this._type_conversions[group] = convert_first(type_converter);
                } else if (type == "n") {
                    s = @"\d{1,3}([,.]\d{3})*";
                    this._group_index += 1;
                    this._type_conversions[group] = int_convert(10);
                } else if (type == "b") {
                    s = @"(0[bB])?[01]+";
                    this._type_conversions[group] = int_convert(2);
                    this._group_index += 1;
                } else if (type == "o") {
                    s = @"(0[oO])?[0-7]+";
                    this._type_conversions[group] = int_convert(8);
                    this._group_index += 1;
                } else if (type == "x") {
                    s = @"(0[xX])?[0-9a-fA-F]+";
                    this._type_conversions[group] = int_convert(16);
                    this._group_index += 1;
                } else if (type == "%") {
                    s = @"\d+(\.\d+)?%";
                    this._group_index += 1;
                    this._type_conversions[group] = percentage;
                } else if (type == "f") {
                    s = @"\d*\.\d+";
                    this._type_conversions[group] = convert_first(float);
                } else if (type == "F") {
                    s = @"\d*\.\d+";
                    this._type_conversions[group] = convert_first(Decimal);
                } else if (type == "e") {
                    s = @"\d*\.\d+[eE][-+]?\d+|nan|NAN|[-+]?inf|[-+]?INF";
                    this._type_conversions[group] = convert_first(float);
                } else if (type == "g") {
                    s = @"\d+(\.\d+)?([eE][-+]?\d+)?|nan|NAN|[-+]?inf|[-+]?INF";
                    this._group_index += 2;
                    this._type_conversions[group] = convert_first(float);
                } else if (type == "d") {
                    if (format.get("width")) {
                        width = String.Format(@"{1,%s}", Convert.ToInt32(format["width"]));
                    } else {
                        width = "+";
                    }
                    s = @"\d{w}|[-+ ]?0[xX][0-9a-fA-F]{w}|[-+ ]?0[bB][01]{w}|[-+ ]?0[oO][0-7]{w}".format(w: width);
                    this._type_conversions[group] = int_convert();
                } else if (type == "ti") {
                    s = String.Format(@"(\d{4}-\d\d-\d\d)((\s+|T)%s)?(Z|\s*[-+]\d\d:?\d\d)?", TIME_PAT);
                    n = this._group_index;
                    this._type_conversions[group] = partial(date_convert, ymd: n + 1, hms: n + 4, tz: n + 7);
                    this._group_index += 7;
                } else if (type == "tg") {
                    s = String.Format(@"(\d{1,2}[-/](\d{1,2}|%s)[-/]\d{4})(\s+%s)?%s?%s?", ALL_MONTHS_PAT, TIME_PAT, AM_PAT, TZ_PAT);
                    n = this._group_index;
                    this._type_conversions[group] = partial(date_convert, dmy: n + 1, hms: n + 5, am: n + 8, tz: n + 9);
                    this._group_index += 9;
                } else if (type == "ta") {
                    s = String.Format(@"((\d{1,2}|%s)[-/]\d{1,2}[-/]\d{4})(\s+%s)?%s?%s?", ALL_MONTHS_PAT, TIME_PAT, AM_PAT, TZ_PAT);
                    n = this._group_index;
                    this._type_conversions[group] = partial(date_convert, mdy: n + 1, hms: n + 5, am: n + 8, tz: n + 9);
                    this._group_index += 9;
                } else if (type == "te") {
                    // this will allow microseconds through if they're present, but meh
                    s = String.Format(@"(%s,\s+)?(\d{1,2}\s+%s\s+\d{4})\s+%s%s", DAYS_PAT, MONTHS_PAT, TIME_PAT, TZ_PAT);
                    n = this._group_index;
                    this._type_conversions[group] = partial(date_convert, dmy: n + 3, hms: n + 5, tz: n + 8);
                    this._group_index += 8;
                } else if (type == "th") {
                    // slight flexibility here from the stock Apache format
                    s = String.Format(@"(\d{1,2}[-/]%s[-/]\d{4}):%s%s", MONTHS_PAT, TIME_PAT, TZ_PAT);
                    n = this._group_index;
                    this._type_conversions[group] = partial(date_convert, dmy: n + 1, hms: n + 3, tz: n + 6);
                    this._group_index += 6;
                } else if (type == "tc") {
                    s = String.Format(@"(%s)\s+%s\s+(\d{1,2})\s+%s\s+(\d{4})", DAYS_PAT, MONTHS_PAT, TIME_PAT);
                    n = this._group_index;
                    this._type_conversions[group] = partial(date_convert, d_m_y: (n + 4, n + 3, n + 8), hms: n + 5);
                    this._group_index += 8;
                } else if (type == "tt") {
                    s = String.Format(@"%s?%s?%s?", TIME_PAT, AM_PAT, TZ_PAT);
                    n = this._group_index;
                    this._type_conversions[group] = partial(date_convert, hms: n + 1, am: n + 4, tz: n + 5);
                    this._group_index += 5;
                } else if (type == "ts") {
                    s = String.Format(@"%s(\s+)(\d+)(\s+)(\d{1,2}:\d{1,2}:\d{1,2})?", MONTHS_PAT);
                    n = this._group_index;
                    this._type_conversions[group] = partial(date_convert, mm: n + 1, dd: n + 3, hms: n + 5);
                    this._group_index += 5;
                } else if (type == "l") {
                    s = @"[A-Za-z]+";
                } else if (type) {
                    s = String.Format(@"\%s+", type);
                } else if (format.get("precision")) {
                    if (format.get("width")) {
                        s = String.Format(@".{%s,%s}?", format["width"], format["precision"]);
                    } else {
                        s = String.Format(@".{1,%s}?", format["precision"]);
                    }
                } else if (format.get("width")) {
                    s = String.Format(@".{%s,}?", format["width"]);
                } else {
                    s = @".+?";
                }
                var align = format["align"];
                var fill = format["fill"];
                // handle some numeric-specific things like fill and sign
                if (is_numeric) {
                    // prefix with something (align "=" trumps zero)
                    if (align == "=") {
                        // special case - align "=" acts like the zero above but with
                        // configurable fill defaulting to "0"
                        if (!fill) {
                            fill = "0";
                        }
                        s = String.Format(@"%s*", fill) + s;
                    }
                    // allow numbers to be prefixed with a sign
                    s = @"[-+ ]?" + s;
                }
                if (!fill) {
                    fill = " ";
                }
                // Place into a group now - this captures the value we want to keep.
                // Everything else from now is just padding to be stripped off
                if (wrap) {
                    s = wrap % s;
                    this._group_index += 1;
                }
                if (format["width"]) {
                    // all we really care about is that if the format originally
                    // specified a width then there will probably be padding - without
                    // an explicit alignment that'll mean right alignment with spaces
                    // padding
                    if (!align) {
                        align = ">";
                    }
                }
                if (@".\+?*[](){}^$".Contains(fill)) {
                    fill = "\\" + fill;
                }
                // align "=" has been handled
                if (align == "<") {
                    s = String.Format("%s%s*", s, fill);
                } else if (align == ">") {
                    s = String.Format("%s*%s", fill, s);
                } else if (align == "^") {
                    s = String.Format("%s*%s%s*", fill, s, fill);
                }
                return s;
            }
        }
        
        // The result of a parse() or search().
        // 
        //     Fixed results may be looked up using `result[index]`.
        //     Slices of fixed results may also be looked up.
        // 
        //     Named results may be looked up using `result['name']`.
        // 
        //     Named results may be tested for existence using `'name' in result`.
        //     
        public class Result
            : object {
            
            public Result(object fixed, object named, object spans) {
                this.fixed = fixed;
                this.named = named;
                this.spans = spans;
            }
            
            public virtual object @__getitem__(object item) {
                if (item is int || item is slice) {
                    return this.fixed[item];
                }
                return this.named[item];
            }
            
            public virtual object @__repr__() {
                return String.Format("<%s %r %r>", this.@__class__.@__name__, this.fixed, this.named);
            }
            
            public virtual object @__contains__(object name) {
                return this.named.Contains(name);
            }
        }
        
        // The result of a parse() or search() if no results are generated.
        // 
        //     This class is only used to expose internal used regex match objects
        //     to the user and use them for external Parser.evaluate_result calls.
        //     
        public class Match
            : object {
            
            public Match(object parser, object match) {
                this.parser = parser;
                this.match = match;
            }
            
            // Generate results for this Match
            public virtual object evaluate_result() {
                return this.parser.evaluate_result(this.match);
            }
        }
        
        // The result of a findall() operation.
        // 
        //     Each element is a Result instance.
        //     
        public class ResultIterator
            : object {
            
            public ResultIterator(
                object parser,
                object @string,
                object pos,
                object endpos,
                object evaluate_result = true) {
                this.parser = parser;
                this.@string = @string;
                this.pos = pos;
                this.endpos = endpos;
                this.evaluate_result = evaluate_result;
            }
            
            public virtual object @__iter__() {
                return this;
            }
            
            public virtual object @__next__() {
                var m = this.parser._search_re.search(this.@string, this.pos, this.endpos);
                if (m == null) {
                    throw StopIteration();
                }
                this.pos = m.end();
                if (this.evaluate_result) {
                    return this.parser.evaluate_result(m);
                } else {
                    return Match(this.parser, m);
                }
            }
            
            public object next = @__next__;
        }
        
        // Using "format" attempt to pull values from "string".
        // 
        //     The format must match the string contents exactly. If the value
        //     you're looking for is instead just a part of the string use
        //     search().
        // 
        //     If ``evaluate_result`` is True the return value will be an Result instance with two attributes:
        // 
        //      .fixed - tuple of fixed-position values from the string
        //      .named - dict of named values from the string
        // 
        //     If ``evaluate_result`` is False the return value will be a Match instance with one method:
        // 
        //      .evaluate_result() - This will return a Result instance like you would get
        //                           with ``evaluate_result`` set to True
        // 
        //     The default behaviour is to match strings case insensitively. You may match with
        //     case by specifying case_sensitive=True.
        // 
        //     If the format is invalid a ValueError will be raised.
        // 
        //     See the module documentation for the use of "extra_types".
        // 
        //     In the case there is no match parse() will return None.
        //     
        public static object parse(
            object format,
            object @string,
            object extra_types = null,
            object evaluate_result = true,
            object case_sensitive = false) {
            var p = Parser(format, extra_types: extra_types, case_sensitive: case_sensitive);
            return p.parse(@string, evaluate_result: evaluate_result);
        }
        
        // Search "string" for the first occurrence of "format".
        // 
        //     The format may occur anywhere within the string. If
        //     instead you wish for the format to exactly match the string
        //     use parse().
        // 
        //     Optionally start the search at "pos" character index and limit the search
        //     to a maximum index of endpos - equivalent to search(string[:endpos]).
        // 
        //     If ``evaluate_result`` is True the return value will be an Result instance with two attributes:
        // 
        //      .fixed - tuple of fixed-position values from the string
        //      .named - dict of named values from the string
        // 
        //     If ``evaluate_result`` is False the return value will be a Match instance with one method:
        // 
        //      .evaluate_result() - This will return a Result instance like you would get
        //                           with ``evaluate_result`` set to True
        // 
        //     The default behaviour is to match strings case insensitively. You may match with
        //     case by specifying case_sensitive=True.
        // 
        //     If the format is invalid a ValueError will be raised.
        // 
        //     See the module documentation for the use of "extra_types".
        // 
        //     In the case there is no match parse() will return None.
        //     
        public static object search(
            object format,
            object @string,
            object pos = 0,
            object endpos = null,
            object extra_types = null,
            object evaluate_result = true,
            object case_sensitive = false) {
            var p = Parser(format, extra_types: extra_types, case_sensitive: case_sensitive);
            return p.search(@string, pos, endpos, evaluate_result: evaluate_result);
        }
        
        // Search "string" for all occurrences of "format".
        // 
        //     You will be returned an iterator that holds Result instances
        //     for each format match found.
        // 
        //     Optionally start the search at "pos" character index and limit the search
        //     to a maximum index of endpos - equivalent to search(string[:endpos]).
        // 
        //     If ``evaluate_result`` is True each returned Result instance has two attributes:
        // 
        //      .fixed - tuple of fixed-position values from the string
        //      .named - dict of named values from the string
        // 
        //     If ``evaluate_result`` is False each returned value is a Match instance with one method:
        // 
        //      .evaluate_result() - This will return a Result instance like you would get
        //                           with ``evaluate_result`` set to True
        // 
        //     The default behaviour is to match strings case insensitively. You may match with
        //     case by specifying case_sensitive=True.
        // 
        //     If the format is invalid a ValueError will be raised.
        // 
        //     See the module documentation for the use of "extra_types".
        //     
        public static object findall(
            object format,
            object @string,
            object pos = 0,
            object endpos = null,
            object extra_types = null,
            object evaluate_result = true,
            object case_sensitive = false) {
            var p = Parser(format, extra_types: extra_types, case_sensitive: case_sensitive);
            return p.findall(@string, pos, endpos, evaluate_result: evaluate_result);
        }
        
        // Create a Parser instance to parse "format".
        // 
        //     The resultant Parser has a method .parse(string) which
        //     behaves in the same manner as parse(format, string).
        // 
        //     The default behaviour is to match strings case insensitively. You may match with
        //     case by specifying case_sensitive=True.
        // 
        //     Use this function if you intend to parse many strings
        //     with the same format.
        // 
        //     See the module documentation for the use of "extra_types".
        // 
        //     Returns a Parser instance.
        //     
        public static object compile(object format, object extra_types = null, object case_sensitive = false) {
            return Parser(format, extra_types: extra_types, case_sensitive: case_sensitive);
        }
    }
}
