using WitcherCore.Models;

namespace WitcherCore.Data
{
    public interface ISaveFileRepository
    {
        void InsertMetadata(SaveFileMetadata metadata);
    }
}
