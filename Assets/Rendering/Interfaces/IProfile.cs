/// <summary>
/// Interface for a render features profile (this should contain any settings for the pass and data for the constant)
/// </summary>
/// <typeparam name="TProfileDataStructure">The data to pass to the shader (through a constant buffer)</typeparam>
public interface IProfile<TProfileDataStructure>
{
    /// <summary>
    /// Gets the compute buffer data 
    /// </summary>
    public TProfileDataStructure GetData();
}