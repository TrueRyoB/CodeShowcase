namespace Fujin.Constants
{
    public class LayerTag
    {
        /// <summary>
        /// Every block that does not suite for a player to escape once stuck inside terrain
        /// example: terrain, lift
        /// </summary>
        public const string Terrain = "Terrain";
        
        /// <summary>
        /// Rare blocks that is an airspace to escape for a playable character despite its normal functionality
        /// example: box
        /// </summary>
        public const string SemiTerrain = "SemiTerrain";
    }
}