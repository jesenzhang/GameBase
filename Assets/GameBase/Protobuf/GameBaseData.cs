﻿// Classes and structures being serialized

// Generated by ProtocolBuffer
// - a pure c# code generation implementation of protocol buffers
// Report bugs to: https://silentorbit.com/protobuf/

// DO NOT EDIT
// This file will be overwritten when CodeGenerator is run.
// To make custom modifications, edit the .proto file and add //:external before the message line
// then write the code and the changes in a separate file.
using System;
using System.Collections.Generic;

namespace Example
{
    public partial class VersionFile
    {
        public enum Type
        {
            DEFAULT = 0,
            COMBINE_FILE = 1,
            RELATION_FILE = 2,
        }

        public string Origin { get; set; }

        public string Guid { get; set; }

        public string Md5 { get; set; }

        public int Size { get; set; }

        public bool Encrypt { get; set; }

        public Example.VersionFile.Type type { get; set; }

        public List<string> Childs { get; set; }

    }

    public partial class VersionInfo
    {
        public long Version { get; set; }

        public List<Example.VersionFile> Files { get; set; }

    }

    public partial class VersionForCheck
    {
        public long Version { get; set; }

        public string Md5 { get; set; }

    }

    public partial class PackItem
    {
        public int X { get; set; }

        public int Y { get; set; }

        public int Width { get; set; }

        public int Height { get; set; }

    }

    public partial class TexturePacker
    {
        public List<string> Names { get; set; }

        public List<Example.PackItem> Items { get; set; }

    }

    public partial class Vector2i
    {
        public int X { get; set; }

        public int Y { get; set; }

    }

    public partial class Vector3f
    {
        public float X { get; set; }

        public float Y { get; set; }

        public float Z { get; set; }

    }

    public partial class Vector2f
    {
        public float X { get; set; }

        public float Y { get; set; }

    }

    public partial class RectF
    {
        public float X { get; set; }

        public float Y { get; set; }

        public float Width { get; set; }

        public float Height { get; set; }

    }

    public partial class ContentValue
    {
        public bool BoolValue { get; set; }

        public int IntValue { get; set; }

        public string StrValue { get; set; }

        public float FloatValue { get; set; }

        public Example.Vector3f Vector3Value { get; set; }

    }

    public partial class Combineinfo
    {
        public int Dir { get; set; }

        public int Start { get; set; }

        public int Size { get; set; }

        public bool Encrypt { get; set; }

    }

    public partial class Groupcombine
    {
        public List<Example.Combineinfo> Infos { get; set; }

    }

    public partial class Stringvalue
    {
        public string Value { get; set; }

    }

    public partial class Combinefiles
    {
        public List<Example.Stringvalue> Files { get; set; }

        public List<Example.Stringvalue> Dirs { get; set; }

        public List<Example.Groupcombine> Groups { get; set; }

    }

    public partial class RelationNode
    {
        public string File { get; set; }

        public List<Example.RelationNode> Nodes { get; set; }

    }

    public partial class RelationFile
    {
        public string Name { get; set; }

        public List<Example.RelationNode> Nodes { get; set; }

    }

}
