use crate::element::{Document, Element};

pub fn parse(input: &str) -> Result<Document, String> {
    let mut parser = Parser::new(input);
    let root = parser.parse_element()?;
    parser.skip_whitespace();
    if parser.peek().is_some() {
        return Err("trailing characters".into());
    }
    Ok(Document { root })
}

struct Parser<'a> {
    chars: std::str::Chars<'a>,
    peeked: Option<Option<char>>,
}

impl<'a> Parser<'a> {
    fn new(input: &'a str) -> Self {
        Parser { chars: input.chars(), peeked: None }
    }

    fn peek(&mut self) -> Option<char> {
        if let Some(p) = self.peeked {
            return p;
        }
        self.peeked = Some(self.chars.next());
        self.peeked.unwrap()
    }

    fn next_char(&mut self) -> Option<char> {
        if let Some(p) = self.peeked.take() {
            return p;
        }
        self.chars.next()
    }

    fn skip_whitespace(&mut self) {
        while let Some(c) = self.peek() {
            if c.is_whitespace() { self.next_char(); } else { break; }
        }
    }

    fn parse_element(&mut self) -> Result<Element, String> {
        self.skip_whitespace();
        match self.peek().ok_or("unexpected end of input")? {
            '"' => self.parse_string(),
            '#' => self.parse_integer(),
            '^' | '*' => self.parse_double(),
            '~' => self.parse_bool(),
            '?' => { self.next_char(); Ok(Element::Null) },
            '[' => self.parse_array(),
            '{' => self.parse_object(),
            _ => {
                // Try integer without specifier
                if self.peek().unwrap().is_ascii_digit() || self.peek().unwrap()=='-' {
                    self.parse_integer_implicit()
                } else {
                    Err(format!("unexpected character '{}'", self.peek().unwrap()))
                }
            }
        }
    }

    fn parse_string(&mut self) -> Result<Element, String> {
        self.next_char();
        let mut s = String::new();
        while let Some(c) = self.next_char() {
            if c == '"' { return Ok(Element::String(s)); }
            s.push(c);
        }
        Err("unterminated string".into())
    }

    fn parse_integer(&mut self) -> Result<Element, String> {
        self.next_char();
        let mut s = String::new();
        while let Some(c) = self.peek() {
            if c.is_ascii_digit() || c=='-' { s.push(c); self.next_char(); } else { break; }
        }
        let v: i64 = s.parse::<i64>().map_err(|e| e.to_string())?;
        Ok(Element::Integer(v))
    }

    fn parse_integer_implicit(&mut self) -> Result<Element, String> {
        let mut s = String::new();
        while let Some(c) = self.peek() {
            if c.is_ascii_digit() || c=='-' { s.push(c); self.next_char(); } else { break; }
        }
        let v: i64 = s.parse::<i64>().map_err(|e| e.to_string())?;
        Ok(Element::Integer(v))
    }

    fn parse_double(&mut self) -> Result<Element, String> {
        self.next_char();
        let mut s = String::new();
        while let Some(c) = self.peek() {
            if c.is_ascii_digit() || c=='.' || c=='-' { s.push(c); self.next_char(); } else { break; }
        }
        let v: f64 = s.parse::<f64>().map_err(|e| e.to_string())?;
        Ok(Element::Double(v))
    }

    fn parse_bool(&mut self) -> Result<Element, String> {
        self.next_char();
        let mut s = String::new();
        while let Some(c) = self.peek() {
            if c.is_alphabetic() { s.push(c); self.next_char(); } else { break; }
        }
        match s.as_str() {
            "true" => Ok(Element::Boolean(true)),
            "false" => Ok(Element::Boolean(false)),
            _ => Err("invalid boolean".into()),
        }
    }

    fn parse_array(&mut self) -> Result<Element, String> {
        self.next_char();
        let mut items = Vec::new();
        loop {
            self.skip_whitespace();
            if let Some(']') = self.peek() { self.next_char(); break; }
            let item = self.parse_element()?;
            items.push(item);
            self.skip_whitespace();
        }
        Ok(Element::Array(items))
    }

    fn parse_object(&mut self) -> Result<Element, String> {
        self.next_char();
        let mut pairs = Vec::new();
        loop {
            self.skip_whitespace();
            if let Some('}') = self.peek() { self.next_char(); break; }
            let key = self.parse_identifier()?;
            self.skip_whitespace();
            let value = self.parse_element()?;
            pairs.push((key, value));
            self.skip_whitespace();
        }
        Ok(Element::Object(pairs))
    }

    fn parse_identifier(&mut self) -> Result<String, String> {
        let mut s = String::new();
        while let Some(c) = self.peek() {
            if c.is_alphanumeric() || c == '_' { s.push(c); self.next_char(); } else { break; }
        }
        if s.is_empty() { Err("expected identifier".into()) } else { Ok(s) }
    }
}
