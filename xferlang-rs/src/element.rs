#[derive(Debug, Clone, PartialEq)]
pub enum Element {
    Null,
    Boolean(bool),
    Integer(i64),
    Double(f64),
    String(String),
    Array(Vec<Element>),
    Object(Vec<(String, Element)>),
}

#[derive(Debug, Clone, PartialEq)]
pub struct Document {
    pub root: Element,
}

