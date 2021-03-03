using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ExamProject.Models
{
    public class Association
    {
        [Key]
        public int AssociationId {get; set;}

        public int ParticipantId {get; set;}

        public User Participant {get; set;}

        public int DojoEventId {get; set;}

        public DojoActivity DojoEvent {get; set;}
    }
}