use std::ffi::{CStr, CString};
use std::os::raw::c_char;
use std::iter::Peekable;
use std::str::Chars;

use serde::Serialize;

#[derive(Debug, Serialize)]
pub enum XferValue {
    String(String),
    Integer(i64),
    Bool(bool),
    List(Vec<XferValue>),
}

#[derive(Debug, Serialize)]
pub struct XferDocument {
    pub values: Vec<XferValue>,
}

fn parse_document(input: &str) -> Result<XferDocument, String> {
    let mut chars = input.chars().peekable();
    let mut values = Vec::new();
    while let Some(_) = chars.peek() {
        skip_whitespace(&mut chars);
        if chars.peek().is_none() { break; }
        values.push(parse_value(&mut chars)?);
        skip_whitespace(&mut chars);
    }
    Ok(XferDocument { values })
}

fn skip_whitespace(chars: &mut Peekable<Chars<'_>>) {
    while matches!(chars.peek(), Some(c) if c.is_whitespace()) {
        chars.next();
    }
}

fn parse_value(chars: &mut Peekable<Chars<'_>>) -> Result<XferValue, String> {
    match chars.peek() {
        Some('"') => parse_string(chars),
        Some('~') => parse_bool(chars),
        Some('[') => parse_list(chars),
        Some(c) if c.is_ascii_digit() || *c == '-' => parse_integer(chars),
        other => Err(format!("Unexpected character {:?}", other)),
    }
}

fn parse_string(chars: &mut Peekable<Chars<'_>>) -> Result<XferValue, String> {
    assert_eq!(chars.next(), Some('"'));
    let mut s = String::new();
    while let Some(c) = chars.next() {
        if c == '"' {
            return Ok(XferValue::String(s));
        } else {
            s.push(c);
        }
    }
    Err("Unterminated string".into())
}

fn parse_bool(chars: &mut Peekable<Chars<'_>>) -> Result<XferValue, String> {
    assert_eq!(chars.next(), Some('~'));
    let mut word = String::new();
    while let Some(c) = chars.peek() {
        if c.is_alphanumeric() {
            word.push(*c);
            chars.next();
        } else {
            break;
        }
    }
    match word.as_str() {
        "true" => Ok(XferValue::Bool(true)),
        "false" => Ok(XferValue::Bool(false)),
        _ => Err(format!("Invalid boolean literal ~{}", word)),
    }
}

fn parse_integer(chars: &mut Peekable<Chars<'_>>) -> Result<XferValue, String> {
    let mut num_str = String::new();
    if let Some('-') = chars.peek() {
        num_str.push('-');
        chars.next();
    }
    while let Some(c) = chars.peek() {
        if c.is_ascii_digit() {
            num_str.push(*c);
            chars.next();
        } else {
            break;
        }
    }
    match num_str.parse::<i64>() {
        Ok(v) => Ok(XferValue::Integer(v)),
        Err(e) => Err(format!("Invalid integer {}: {}", num_str, e)),
    }
}

fn parse_list(chars: &mut Peekable<Chars<'_>>) -> Result<XferValue, String> {
    assert_eq!(chars.next(), Some('['));
    let mut values = Vec::new();
    loop {
        skip_whitespace(chars);
        if let Some(']') = chars.peek() {
            chars.next();
            break;
        }
        values.push(parse_value(chars)?);
        skip_whitespace(chars);
    }
    Ok(XferValue::List(values))
}

#[no_mangle]
pub unsafe extern "C" fn xfer_parse(input: *const c_char) -> *mut XferDocument {
    if input.is_null() { return std::ptr::null_mut(); }
    let c_str = unsafe { CStr::from_ptr(input) };
    let s = match c_str.to_str() {
        Ok(v) => v,
        Err(_) => return std::ptr::null_mut(),
    };
    match parse_document(s) {
        Ok(doc) => Box::into_raw(Box::new(doc)),
        Err(_) => std::ptr::null_mut(),
    }
}

#[no_mangle]
pub unsafe extern "C" fn xfer_document_free(ptr: *mut XferDocument) {
    if !ptr.is_null() {
        unsafe { drop(Box::from_raw(ptr)); }
    }
}

#[no_mangle]
pub unsafe extern "C" fn xfer_document_to_json(ptr: *const XferDocument) -> *mut c_char {
    if ptr.is_null() { return std::ptr::null_mut(); }
    let doc = unsafe { &*ptr };
    match serde_json::to_string(doc) {
        Ok(json) => match CString::new(json) {
            Ok(c_string) => c_string.into_raw(),
            Err(_) => std::ptr::null_mut(),
        },
        Err(_) => std::ptr::null_mut(),
    }
}

#[no_mangle]
pub unsafe extern "C" fn xfer_string_free(ptr: *mut c_char) {
    if !ptr.is_null() {
        unsafe { drop(CString::from_raw(ptr)); }
    }
}

#[cfg(test)]
mod tests {
    use super::*;

    #[test]
    fn parse_basic() {
        let doc = parse_document("\"hello\" 42 ~true [1 2]").unwrap();
        assert_eq!(doc.values.len(), 4);
    }
}

