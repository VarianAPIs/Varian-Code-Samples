using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helpers
{
    public class ArgumentValidatorFactory
    {
        public IArgumentValidator CreateArgumentValidator(bool STSMode)
        {
            if (STSMode)
                return new TokenValidator();
            else
                return new ADValidator();
        }
    }
}
