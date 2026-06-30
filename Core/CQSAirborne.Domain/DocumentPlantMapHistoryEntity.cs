using System;
using System.Collections.Generic;
using System.Text;

namespace CQSAirborne.Domain
{
    public class DocumentPlantMapHistoryEntity : BaseEntity
    {
        public int Id { get; set; }
        public int DocumentHistoryId { get; set; }
        public int PlantId { get; set; }
        public long PictureId { get; set; }

        public virtual DocumentHistoryEntity Document { get; set; }
        public virtual PlantEntity Plant { get; set; }

        public virtual PictureEntity Picture { get; set; }
    }
}
