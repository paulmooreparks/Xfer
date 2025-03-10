﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



namespace ParksComputing.XferKit.Cli;
internal class PersistenceService {
    private readonly string _xfercDirectory;
    private readonly Mutex _mutex;

    public PersistenceService() {
        var homeDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        _xfercDirectory = Path.Combine(homeDirectory, XferKit.Workspace.Constants.XferDirectoryName);

        if (!Directory.Exists(_xfercDirectory)) {
            Directory.CreateDirectory(_xfercDirectory);
        }

        _mutex = new Mutex(false, XferKit.Workspace.Constants.MutexName);
    }

}
