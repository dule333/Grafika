using Common.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Common
{
    public class Converter
    {
        public static List<EntityWrapper> powerEntities = new List<EntityWrapper>();

        public static SortedList<double, LineEntity> lineEntities = new SortedList<double, LineEntity>(new CompareHelper<double>());
        public static SortedList<double, LineEntity> lineEntities2 = new SortedList<double, LineEntity>(new CompareHelper<double>());
        private static List<LineEntity> tempList = new List<LineEntity>();
        public static void ToLatLon(double utmX, double utmY, int zoneUTM, out double latitude, out double longitude)
        {
            bool isNorthHemisphere = true;

            var diflat = -0.00066286966871111111111111111111111111;
            var diflon = -0.0003868060578;

            var zone = zoneUTM;
            var c_sa = 6378137.000000;
            var c_sb = 6356752.314245;
            var e2 = Math.Pow((Math.Pow(c_sa, 2) - Math.Pow(c_sb, 2)), 0.5) / c_sb;
            var e2cuadrada = Math.Pow(e2, 2);
            var c = Math.Pow(c_sa, 2) / c_sb;
            var x = utmX - 500000;
            var y = isNorthHemisphere ? utmY : utmY - 10000000;

            var s = ((zone * 6.0) - 183.0);
            var lat = y / (c_sa * 0.9996);
            var v = (c / Math.Pow(1 + (e2cuadrada * Math.Pow(Math.Cos(lat), 2)), 0.5)) * 0.9996;
            var a = x / v;
            var a1 = Math.Sin(2 * lat);
            var a2 = a1 * Math.Pow((Math.Cos(lat)), 2);
            var j2 = lat + (a1 / 2.0);
            var j4 = ((3 * j2) + a2) / 4.0;
            var j6 = ((5 * j4) + Math.Pow(a2 * (Math.Cos(lat)), 2)) / 3.0;
            var alfa = (3.0 / 4.0) * e2cuadrada;
            var beta = (5.0 / 3.0) * Math.Pow(alfa, 2);
            var gama = (35.0 / 27.0) * Math.Pow(alfa, 3);
            var bm = 0.9996 * c * (lat - alfa * j2 + beta * j4 - gama * j6);
            var b = (y - bm) / v;
            var epsi = ((e2cuadrada * Math.Pow(a, 2)) / 2.0) * Math.Pow((Math.Cos(lat)), 2);
            var eps = a * (1 - (epsi / 3.0));
            var nab = (b * (1 - epsi)) + lat;
            var senoheps = (Math.Exp(eps) - Math.Exp(-eps)) / 2.0;
            var delt = Math.Atan(senoheps / (Math.Cos(nab)));
            var tao = Math.Atan(Math.Cos(delt) * Math.Tan(nab));

            longitude = ((delt * (180.0 / Math.PI)) + s) + diflon;
            latitude = ((lat + (1 + e2cuadrada * Math.Pow(Math.Cos(lat), 2) - (3.0 / 2.0) * e2cuadrada * Math.Sin(lat) * Math.Cos(lat) * (tao - lat)) * (tao - lat)) * (180.0 / Math.PI)) + diflat;
        }
        public void Convert()
        {
            powerEntities.Clear();
            lineEntities.Clear();
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load("Geographic.xml");

            MapSubstations(xmlDoc);
            MapSwitches(xmlDoc);
            MapNodes(xmlDoc);
            MapLines(xmlDoc);    
            
        }

        public void SetBounds(out double maxX, out double maxY, out double minX, out double minY )
        {
            maxX = powerEntities.Max(x => x.powerEntity.X) + 0.001;
            maxY = powerEntities.Max(x => x.powerEntity.Y) + 0.001;
            minX = powerEntities.Min(x => x.powerEntity.X) - 0.001;
            minY = powerEntities.Min(x => x.powerEntity.Y) - 0.001;
        }
        

        private void MapSubstations(XmlDocument xmlDoc)
        {
            XmlNodeList nodeList;
            EntityWrapper entity;
            SubstationEntity sub;
            double noviX, noviY;

            nodeList = xmlDoc.DocumentElement.SelectNodes("/NetworkModel/Substations/SubstationEntity");
            foreach (XmlNode node in nodeList)
            {
                sub = new SubstationEntity();
                sub.Id = long.Parse(node.SelectSingleNode("Id").InnerText);
                sub.Name = node.SelectSingleNode("Name").InnerText;
                sub.X = double.Parse(node.SelectSingleNode("Y").InnerText);
                sub.Y = double.Parse(node.SelectSingleNode("X").InnerText);

                ToLatLon(sub.X, sub.Y, 34, out noviX, out noviY);
                sub.X = noviX;
                sub.Y = noviY;

                entity = new EntityWrapper();
                entity.powerEntity = sub;
                entity.entityType = EntityType.Substation;
                powerEntities.Add(entity);

            }
        }
        private void MapNodes(XmlDocument xmlDoc)
        {
            XmlNodeList nodeList;
            EntityWrapper entity;
            double noviX, noviY;
            NodeEntity nodeobj = new NodeEntity();

            nodeList = xmlDoc.DocumentElement.SelectNodes("/NetworkModel/Nodes/NodeEntity");
            foreach (XmlNode node in nodeList)
            {
                nodeobj = new NodeEntity();
                nodeobj.Id = long.Parse(node.SelectSingleNode("Id").InnerText);
                nodeobj.Name = node.SelectSingleNode("Name").InnerText;
                nodeobj.X = double.Parse(node.SelectSingleNode("Y").InnerText);
                nodeobj.Y = double.Parse(node.SelectSingleNode("X").InnerText);

                ToLatLon(nodeobj.X, nodeobj.Y, 34, out noviX, out noviY);
                nodeobj.X = noviX;
                nodeobj.Y = noviY;

                entity = new EntityWrapper();
                entity.powerEntity = nodeobj;
                entity.entityType = EntityType.Node;
                powerEntities.Add(entity);
            }
        }
        private void MapSwitches(XmlDocument xmlDoc)
        {
            XmlNodeList nodeList;
            double noviX, noviY;

            SwitchEntity switchobj = new SwitchEntity();

            nodeList = xmlDoc.DocumentElement.SelectNodes("/NetworkModel/Switches/SwitchEntity");
            foreach (XmlNode node in nodeList)
            {
                switchobj = new SwitchEntity();
                switchobj.Id = long.Parse(node.SelectSingleNode("Id").InnerText);
                switchobj.Name = node.SelectSingleNode("Name").InnerText;
                switchobj.X = double.Parse(node.SelectSingleNode("Y").InnerText);
                switchobj.Y = double.Parse(node.SelectSingleNode("X").InnerText);

                ToLatLon(switchobj.X, switchobj.Y, 34, out noviX, out noviY);
                switchobj.X = noviX;
                switchobj.Y = noviY;

                EntityWrapper entity = new EntityWrapper();
                entity.powerEntity = switchobj;
                entity.entityType = EntityType.Switch;
                powerEntities.Add(entity);
            }
        }
        private void MapLines(XmlDocument xmlDoc)
        {
            XmlNodeList nodeList;
            LineEntity l = new LineEntity();

            nodeList = xmlDoc.DocumentElement.SelectNodes("/NetworkModel/Lines/LineEntity");
            foreach (XmlNode node in nodeList)
            {
                l = new LineEntity();
                l.Id = long.Parse(node.SelectSingleNode("Id").InnerText);
                l.Name = node.SelectSingleNode("Name").InnerText;
                l.FirstEnd = long.Parse(node.SelectSingleNode("FirstEnd").InnerText);
                l.SecondEnd = long.Parse(node.SelectSingleNode("SecondEnd").InnerText);
                if (l.FirstEnd == 0 || l.SecondEnd == 0)
                    continue;
                EntityWrapper first = powerEntities.Find(t => t.powerEntity.Id == l.FirstEnd), second = powerEntities.Find(t => t.powerEntity.Id == l.SecondEnd);
                if (first != null && second != null)
                {
                    if (tempList.Find(t => (t.FirstEnd == l.FirstEnd && t.SecondEnd == l.SecondEnd) || (t.FirstEnd == l.SecondEnd && t.SecondEnd == l.FirstEnd)) != null)
                    {
                        continue;
                    }
                    tempList.Add(l);
                    lineEntities.Add(Math.Sqrt((second.powerEntity.X - first.powerEntity.X) * (second.powerEntity.X - first.powerEntity.X) +
                (second.powerEntity.Y - first.powerEntity.Y) * (second.powerEntity.Y - first.powerEntity.Y)), l);
                }
            }
            tempList.Clear();
        }
    }
}
