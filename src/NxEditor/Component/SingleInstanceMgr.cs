﻿using System.IO.Pipes;
using System.Text;

namespace NxEditor.Component;

public static class SingleInstanceMgr
{
    private static readonly string _appId = "NX-Editor(cf981f5b-5063-46a3-9e98-219f25ec0005)";
    private static Action<string[]>? _attach;

    public static bool Start(string[] args, Action<string[]> attach)
    {
        _attach = attach;

        using NamedPipeClientStream client = new(_appId);
        try {
            client.Connect(0);
        }
        catch {
            Task.Run(StartServerListener);
            return true;
        }

        using BinaryWriter writer = new(client, Encoding.UTF8);

        writer.Write(args.Length);
        for (int i = 0; i < args.Length; i++) {
            writer.Write(args[i]);
        }

        return false;
    }

    public static void StartServerListener()
    {
        NamedPipeServerStream server = new(_appId);
        server.WaitForConnection();

        using (var reader = new BinaryReader(server, Encoding.UTF8)) {
            int argc = reader.ReadInt32();
            string[] args = new string[argc];
            for (int i = 0; i < argc; i++) {
                args[i] = reader.ReadString();
            }

            if (args.Length > 0) {
                _attach?.Invoke(args);
            }
        }

        StartServerListener();
    }
}
