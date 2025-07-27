using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ParksComputing.Xfer.Lang.Elements;

namespace ParksComputing.Xfer.Lang;

internal class XferTextWriter : IDisposable {
    private bool _isClosed = false;
    private readonly Formatting _formatting;
    private readonly TextWriter _writer;
    private int _indentation = 4;
    private char _indentChar = ' ';

    public int Indentation {
        get => _indentation;
        set {
            if (value < 0) {
                throw new ArgumentOutOfRangeException(nameof(value), "Indentation must be greater than or equal to zero.");
            }
            _indentation = value;
        }
    }

    public char IndentChar {
        get => _indentChar;
        set {
            if (value == '\0') {
                throw new ArgumentException("IndentChar cannot be the null character.", nameof(value));
            }
            _indentChar = value;
        }
    }

    protected virtual void Dispose(bool disposing) {
        if (!_isClosed) {
            if (disposing) {
                Close();
            }
        }
    }

    public XferTextWriter(TextWriter writer, Formatting formatting = Formatting.None) {
        _formatting = formatting;
        _writer = writer;
    }

    public void Write(XferDocument xferDocument) {
        if (_isClosed) {
            throw new ObjectDisposedException(GetType().Name);
        }

        foreach (var meta in xferDocument.MetadataCollection) {
            Write(meta);
        }
        Write(xferDocument.Root);
    }

    public void Write(Element element) {
        if (_isClosed) {
            throw new ObjectDisposedException(GetType().Name);
        }

        _writer.Write(element.ToXfer());
    }

    public void Dispose() {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    void Close() {
        _isClosed = true;
    }
}
