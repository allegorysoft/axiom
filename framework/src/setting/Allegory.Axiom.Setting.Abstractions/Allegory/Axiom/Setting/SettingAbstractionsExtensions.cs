using System.Threading.Tasks;
using Allegory.Axiom.Exceptions;

namespace Allegory.Axiom.Setting;

public static class SettingAbstractionsExtensions
{
    extension(ISettingDefinitionStore store)
    {
        public async ValueTask<SettingDefinition> GetAsync(string name)
        {
            var definition = await store.FindAsync(name);

            if (definition == null)
            {
                throw new NotFoundException(SettingExceptionCodes.DefinitionNotFound)
                    .AddData("name", name);
            }

            return definition;
        }
    }

    extension(ISettingValueManager manager)
    {
        public async ValueTask<T> GetAsync<T>(string name)
        {
            var value = await manager.FindAsync<T>(name);

            if (value == null)
            {
                throw new NotFoundException(SettingExceptionCodes.ValueNotFound)
                    .AddData("name", name);
            }

            return value;
        }

        public async ValueTask<SettingValue> GetAsync(string name)
        {
            var value = await manager.FindAsync(name);

            if (value == null)
            {
                throw new NotFoundException(SettingExceptionCodes.ValueNotFound)
                    .AddData("name", name);
            }

            return value;
        }
    }
}