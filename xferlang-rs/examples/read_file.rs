use std::env;
use std::fs;

fn main() {
    let args: Vec<String> = env::args().collect();
    if args.len() != 2 {
        eprintln!("Usage: {} <file>", args[0]);
        std::process::exit(1);
    }
    let data = fs::read_to_string(&args[1]).expect("failed to read file");
    match xferlang_rs::parse(&data) {
        Ok(doc) => println!("{:#?}", doc),
        Err(e) => eprintln!("parse error: {}", e),
    }
}
