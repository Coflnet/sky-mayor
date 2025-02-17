using System.Runtime.Serialization;

namespace Coflnet.Sky.Mayor.Models
{
    /// <summary>
    /// 
    /// </summary>
    public class ModelWinner : ModelCandidate
    {
        /// <summary>
        /// Gets or Sets the minister
        /// </summary>
        [DataMember(Name="minister", EmitDefaultValue=false)]
        public Minister Minister { get; set; }
    }
}
