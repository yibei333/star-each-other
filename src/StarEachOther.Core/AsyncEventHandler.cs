using System.Threading.Tasks;

namespace StarEachOther.Core;

public delegate Task AsyncEventHandler<T>(T arg);
