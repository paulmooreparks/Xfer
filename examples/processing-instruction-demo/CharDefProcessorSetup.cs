using ParksComputing.Xfer.Lang.CharDef;
using ParksComputing.Xfer.Lang.Services;
using ParksComputing.Xfer.Lang.Elements;

namespace ProcessingInstructionDemo {
    public static class CharDefProcessorSetup {
        public static CharDefProcessor RegisterWith(Parser parser) {
            var processor = new CharDefProcessor();
            parser.RegisterPIProcessor(CharDefProcessor.CharDefKey, processor.PIHandler);
            parser.RegisterElementProcessor(processor.ElementHandler);
            return processor;
        }
    }
}
