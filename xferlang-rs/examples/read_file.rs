use std::env;
use std::fs;
use std::path::Path;

fn main() {
    let mut args = env::args_os();
    let exe = args.next().unwrap_or_default();
    let Some(path) = args.next() else {
        eprintln!("Usage: {:?} <file>", exe);
        std::process::exit(1);
    };

    let data = match fs::read_to_string(Path::new(&path)) {
        Ok(d) => d,
        Err(e) => {
            eprintln!("failed to read {:?}: {}", path, e);
            std::process::exit(1);
        }
    };

    match xferlang_rs::parse(&data) {
        Ok(doc) => println!("{:#?}", doc),
        Err(e) => {
            eprintln!("parse error: {}", e);
            std::process::exit(1);
        }
    }
}
