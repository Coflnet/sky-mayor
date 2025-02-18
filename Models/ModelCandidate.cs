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
    /// <summary>
    /// 
    /// </summary>
    [DataContract]
    public partial class ModelCandidate : IEquatable<ModelCandidate>
    {
        /// <summary>
        /// Gets or Sets Key
        /// </summary>
        [DataMember(Name = "key", EmitDefaultValue = false)]
        public string Key { get; set; }

        /// <summary>
        /// Gets or Sets Name
        /// </summary>
        [DataMember(Name = "name", EmitDefaultValue = false)]
        public string Name { get; set; }

        /// <summary>
        /// Gets or Sets Perks
        /// </summary>
        [DataMember(Name = "perks", EmitDefaultValue = false)]
        public List<ModelPerk> Perks { get; set; }
        /// <summary>
        /// Gets or Sets Votes candidate received
        /// </summary>
        [DataMember(Name = "votes", EmitDefaultValue = false)]
        public int Votes { get; set; }

        /// <summary>
        /// Returns the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("class ModelCandidate {\n");
            sb.Append("  Key: ").Append(Key).Append("\n");
            sb.Append("  Name: ").Append(Name).Append("\n");
            sb.Append("  Perks: ").Append(Perks).Append("\n");
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
            return obj.GetType() == GetType() && Equals((ModelCandidate)obj);
        }

        /// <summary>
        /// Returns true if ModelCandidate instances are equal
        /// </summary>
        /// <param name="other">Instance of ModelCandidate to be compared</param>
        /// <returns>Boolean</returns>
        public bool Equals(ModelCandidate other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;

            return
                (
                    Key == other.Key ||
                    Key != null &&
                    Key.Equals(other.Key)
                ) &&
                (
                    Name == other.Name ||
                    Name != null &&
                    Name.Equals(other.Name)
                ) &&
                (
                    Perks == other.Perks ||
                    Perks != null &&
                    other.Perks != null &&
                    Perks.SequenceEqual(other.Perks)
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
                if (Key != null)
                    hashCode = hashCode * 59 + Key.GetHashCode();
                if (Name != null)
                    hashCode = hashCode * 59 + Name.GetHashCode();
                if (Perks != null)
                    hashCode = hashCode * 59 + Perks.GetHashCode();
                return hashCode;
            }
        }

        #region Operators
#pragma warning disable 1591

        public static bool operator ==(ModelCandidate left, ModelCandidate right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(ModelCandidate left, ModelCandidate right)
        {
            return !Equals(left, right);
        }

#pragma warning restore 1591
        #endregion Operators
    }
}
