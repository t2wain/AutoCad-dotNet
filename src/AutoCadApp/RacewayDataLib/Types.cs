using CsvHelper.Configuration;
using System.Collections.Generic;

namespace RacewayDataLib
{
    public class Raceway
    {
        public int ID { get; set; }
        public string Tag { get; set; }
        public int FromNodeID { get; set; }
        public Node FromNode { get; set; }
        public int ToNodeID { get; set; }
        public Node ToNode { get; set; }
        public double Length { get; set; }
        public string Type { get; set; }
        public int IsDrop { get; set; }
        public int Sys1 { get; set; }
        public int Sys2 { get; set; }
        public int Sys3 { get; set; }
        public int Sys4 { get; set; }
        public int Sys5 { get; set; }
        public int Sys6 { get; set; }
        public int Sys7 { get; set; }
        public int Sys8 { get; set; }
        public int Sys9 { get; set; }
        public int Sys10 { get; set; }
        public int Component { get; set; }
        public bool IsSystem(int sysNo)
        {
            return this.Sys1 == sysNo
                   || this.Sys2 == sysNo
                   || this.Sys3 == sysNo
                   || this.Sys4 == sysNo
                   || this.Sys5 == sysNo
                   || this.Sys6 == sysNo
                   || this.Sys7 == sysNo
                   || this.Sys8 == sysNo
                   || this.Sys9 == sysNo
                   || this.Sys10 == sysNo;
        }

        HashSet<int> _hsys = new HashSet<int>();
        bool _isinit = false;
        public HashSet<int> Systems
        {
            get
            {
                //HashSet<int> l = new HashSet<int>();

                if (!this._isinit)
                {
                    var l = this._hsys;
                    l.Add(this.Sys1);
                    l.Add(this.Sys2);
                    l.Add(this.Sys3);
                    l.Add(this.Sys4);
                    l.Add(this.Sys5);
                    l.Add(this.Sys6);
                    l.Add(this.Sys7);
                    l.Add(this.Sys8);
                    l.Add(this.Sys9);
                    l.Add(this.Sys10);
                    l.Remove(-1);
                    this._isinit = true;
                }
                return this._hsys;
            }
        }
    }

    public sealed class RacewayMap : ClassMap<Raceway>
    {
        public RacewayMap()
        {
            Map(m => m.ID).Index(0);
            Map(m => m.Tag).Index(1);
            Map(m => m.FromNodeID).Index(2);
            Map(m => m.ToNodeID).Index(3);
            Map(m => m.Length).Index(4);
            Map(m => m.Type).Index(5);
            Map(m => m.IsDrop).Index(6);
            Map(m => m.Sys1).Index(7).Default(-1);
            Map(m => m.Sys2).Index(8).Default(-1);
            Map(m => m.Sys3).Index(9).Default(-1);
            Map(m => m.Sys4).Index(10).Default(-1);
            Map(m => m.Sys5).Index(11).Default(-1);
            Map(m => m.Sys6).Index(12).Default(-1);
            Map(m => m.Sys7).Index(13).Default(-1);
            Map(m => m.Sys8).Index(14).Default(-1);
            Map(m => m.Sys9).Index(15).Default(-1);
            Map(m => m.Sys10).Index(16).Default(-1);
        }
    }

    public class Cable
    {
        public int ID { get; set; }
        public string Tag { get; set; }
        public int FromNodeID { get; set; }
        public int ToNodeID { get; set; }
        public int SegSystem { get; set; }
        public double RoutedLength { get; set; }
        public int RouteStatus { get; set; }
    }

    public sealed class CableMap : ClassMap<Cable>
    {
        public CableMap()
        {
            Map(m => m.ID).Index(0);
            Map(m => m.Tag).Index(1);
            Map(m => m.FromNodeID).Index(2);
            Map(m => m.ToNodeID).Index(3);
            Map(m => m.SegSystem).Index(4);
            Map(m => m.RoutedLength).Index(5);
            Map(m => m.RouteStatus).Index(6);
        }
    }

    public class Route
    {
        public int CableID { get; set; }
        public int RacewayID { get; set; }
        public int Sequence { get; set; }
        public int FromNodeID { get; set; }
        public int ToNodeID { get; set; }
    }

    public sealed class RouteMap : ClassMap<Route>
    {
        public RouteMap()
        {
            Map(m => m.CableID).Index(0);
            Map(m => m.RacewayID).Index(1);
            Map(m => m.Sequence).Index(2);
            Map(m => m.FromNodeID).Index(3);
            Map(m => m.ToNodeID).Index(4);
        }
    }

    public class SegSystem
    {
        public int ID { get; set; }
        public string Name { get; set; }
    }

    public sealed class SegSystemMap : ClassMap<SegSystem>
    {
        public SegSystemMap()
        {
            Map(m => m.ID).Index(0);
            Map(m => m.Name).Index(1);
        }
    }

    public class Node
    {
        public int ID { get; set; }
        public string Tag { get; set; }
        public string NodeType { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }
    }

    public sealed class NodeMap : ClassMap<Node>
    {
        public NodeMap()
        {
            Map(m => m.ID).Index(0);
            Map(m => m.Tag).Index(1);
            Map(m => m.NodeType).Index(2);
            Map(m => m.X).Index(3);
            Map(m => m.Y).Index(4);
            Map(m => m.Z).Index(5);
        }
    }

}
