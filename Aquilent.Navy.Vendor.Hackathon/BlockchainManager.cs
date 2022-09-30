using Newtonsoft.Json;

namespace Aquilent.Navy.Vendor.Hackathon
{
    public class BlockchainManager
    {
        public Blockchain Deserialize(string blockChainData)
        {

            // Could encrypt this to prevent tampering in the database.
            var temp = JsonConvert.DeserializeObject<Blockchain>(blockChainData);

            return temp;
        }

        public string Serialize(Blockchain block)
        {
            // Could encrypt this to prevent tampering in the database.
            var data = JsonConvert.SerializeObject(block);

            return data;
        }

        // Complicated blockchain
    }
}
