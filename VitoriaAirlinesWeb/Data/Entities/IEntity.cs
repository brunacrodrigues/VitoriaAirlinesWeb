namespace VitoriaAirlinesWeb.Data.Entities
{
    /// <summary>
    /// Represents a basic entity with a unique identifier.
    /// All entities in the system should implement this interface.
    /// </summary>
    public interface IEntity
    {
        /// <summary>
        /// Gets or sets the unique identifier for the entity.
        /// </summary
        public int Id { get; set; }


    }
}
