using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Coflnet.Sky.Mayor.Converters;

namespace Coflnet.Sky.Mayor.Models
{ 
    public class ElectionWinner : ModelCandidate
    {
        public Minister Minister { get; set; }
    }
    /// <summary>
    /// 
    /// </summary>
    [DataContract]
    public partial class ModelElectionPeriod : IEquatable<ModelElectionPeriod>
    {
        /// <summary>
        /// Gets or Sets Candidates
        /// </summary>
        [DataMember(Name="candidates", EmitDefaultValue=false)]
        public List<ModelCandidate> Candidates { get; set; }

        /// <summary>
        /// Gets or Sets End
        /// </summary>
        [DataMember(Name="end", EmitDefaultValue=false)]
        public string End { get; set; }

        /// <summary>
        /// Gets or Sets Id
        /// </summary>
        [DataMember(Name="id", EmitDefaultValue=false)]
        public string Id { get; set; }

        /// <summary>
        /// Gets or Sets Start
        /// </summary>
        [DataMember(Name="start", EmitDefaultValue=false)]
        public string Start { get; set; }

        /// <summary>
        /// Gets or Sets Winner
        /// </summary>
        [DataMember(Name="winner", EmitDefaultValue=false)]
        public ModelCandidate Winner { get; set; }

        /// <summary>
        /// Gets or Sets the minister
        /// </summary>
        [DataMember(Name="minister", EmitDefaultValue=false)]
        public Minister Minister { get; set; }

        /// <summary>
        /// Gets or Sets Year
        /// </summary>
        [DataMember(Name="year", EmitDefaultValue=true)]
        public int Year { get; set; }

        /// <summary>
        /// Returns the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("class ModelElectionPeriod {\n");
            sb.Append("  Candidates: ").Append(Candidates).Append("\n");
            sb.Append("  End: ").Append(End).Append("\n");
            sb.Append("  Id: ").Append(Id).Append("\n");
            sb.Append("  Start: ").Append(Start).Append("\n");
            sb.Append("  Winner: ").Append(Winner).Append("\n");
            sb.Append("  Year: ").Append(Year).Append("\n");
            sb.Append("}\n");
            return sb.ToString();
        }

        /// <summary>
        /// Returns the JSON string presentation of the object
        /// </summary>
        /// <returns>JSON string presentation of the object</returns>
        public string ToJson()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }

        /// <summary>
        /// Returns true if objects are equal
        /// </summary>
        /// <param name="obj">Object to be compared</param>
        /// <returns>Boolean</returns>
        public override bool Equals(object obj)
        {
            if (obj is null) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && Equals((ModelElectionPeriod)obj);
        }

        /// <summary>
        /// Returns true if ModelElectionPeriod instances are equal
        /// </summary>
        /// <param name="other">Instance of ModelElectionPeriod to be compared</param>
        /// <returns>Boolean</returns>
        public bool Equals(ModelElectionPeriod other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;

            return 
                (
                    Candidates == other.Candidates ||
                    Candidates != null &&
                    other.Candidates != null &&
                    Candidates.SequenceEqual(other.Candidates)
                ) && 
                (
                    End == other.End ||
                    End != null &&
                    End.Equals(other.End)
                ) && 
                (
                    Id == other.Id ||
                    Id != null &&
                    Id.Equals(other.Id)
                ) && 
                (
                    Start == other.Start ||
                    Start != null &&
                    Start.Equals(other.Start)
                ) && 
                (
                    Winner == other.Winner ||
                    Winner != null &&
                    Winner.Equals(other.Winner)
                ) && 
                (
                    Year == other.Year ||
                    
                    Year.Equals(other.Year)
                );
        }

        /// <summary>
        /// Gets the hash code
        /// </summary>
        /// <returns>Hash code</returns>
        public override int GetHashCode()
        {
            unchecked // Overflow is fine, just wrap
            {
                var hashCode = 41;
                // Suitable nullity checks etc, of course :)
                    if (Candidates != null)
                    hashCode = hashCode * 59 + Candidates.GetHashCode();
                    if (End != null)
                    hashCode = hashCode * 59 + End.GetHashCode();
                    if (Id != null)
                    hashCode = hashCode * 59 + Id.GetHashCode();
                    if (Start != null)
                    hashCode = hashCode * 59 + Start.GetHashCode();
                    if (Winner != null)
                    hashCode = hashCode * 59 + Winner.GetHashCode();
                    
                    hashCode = hashCode * 59 + Year.GetHashCode();
                return hashCode;
            }
        }

        #region Operators
        #pragma warning disable 1591

        public static bool operator ==(ModelElectionPeriod left, ModelElectionPeriod right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(ModelElectionPeriod left, ModelElectionPeriod right)
        {
            return !Equals(left, right);
        }

        #pragma warning restore 1591
        #endregion Operators
    }
}
