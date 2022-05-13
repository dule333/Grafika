using Common.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public class EntityWrapper
    {
        public PowerEntity powerEntity;
        public LineType lineType;
        public List<LineEntity> lines;
        public List<Tuple<long, long>> lineEntities;
        public bool lineDrawn = false;
        public bool entityPlaced = false;
        public EntityType entityType;
    }
}