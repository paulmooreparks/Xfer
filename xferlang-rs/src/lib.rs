mod element;
mod parser;

pub use element::{Document, Element};

pub fn parse(input: &str) -> Result<Document, String> {
    parser::parse(input)
}

#[cfg(test)]
mod tests {
    use super::*;

    #[test]
    fn parse_simple_example() {
        let input = "{name\"Alice\"age 30 isMember~true scores[*85 *90 *78.5]}";
        let doc = parse(input).expect("parse failed");
        match doc.root {
            Element::Object(ref pairs) => assert_eq!(pairs.len(), 4),
            _ => panic!("not object"),
        }
    }
}
