//
//  Program.cs
//
//  Author:
//       Jarl Gullberg <jarl.gullberg@gmail.com>
//
//  Copyright (c) Jarl Gullberg
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU Lesser General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
//
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU Lesser General Public License for more details.
//
//  You should have received a copy of the GNU Lesser General Public License
//  along with this program.  If not, see <http://www.gnu.org/licenses/>.
//

using System;
using System.IO;
using Statiq.App;
using Statiq.Common;
using Statiq.Docs;
using Statiq.Web;

var rootPath = Path.GetFullPath("../../docs");

await Bootstrapper.Factory
    .CreateDocs(args)
    .ConfigureFileSystem(fs =>
    {
        fs.RootPath = rootPath;
    })
    .ConfigureSettings(settings =>
    {
        // settings[WebKeys.GenerateSearchIndex] = true;
    })

    // .AddSourceFiles
    // (
    //     "../../Backend/*/{!.git,!bin,!obj,!packages,!*.Tests,}/**/*.cs",
    //     "../../Remora.Discord.*/{!.git,!bin,!obj,!packages,!*.Tests,}/**/*.cs"
    // )
    .ConfigureProcesses(processes =>
    {
        processes.AddPreviewProcess
        (
            ProcessTiming.Initialization,
            _ => new ProcessLauncher
            (
                "npm",
                "run",
                "tailwind:preview"
            )
            {
                IsBackground = true,
                WorkingDirectory = rootPath,
                LogErrors = false
            }
        );

        processes.AddNonPreviewProcess
        (
            ProcessTiming.BeforeDeployment,
            true,
            _ => new ProcessLauncher
            (
                "npm",
                "run",
                "tailwind:build"
            )
            {
                WorkingDirectory = rootPath,
                LogErrors = false
            }
        );
    })
    .RunAsync();
