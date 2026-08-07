using System.Collections.Generic;

namespace UnityEditor.Il2Cpp
{
    /// <summary>
    /// Toolchain package implementation for building Linux players on a Linux x86_64 host.
    /// </summary>
    /// <remarks>
    /// This package provides the LLVM/Clang toolchain (compiler, linker, etc.). It does not
    /// include a sysroot; pair it with a matching sysroot package when cross-compiling.
    /// The payload is distributed via UPM under <see cref="_packageName"/> and installed
    /// into the sysroot/toolchain cache by the base <c>SysrootPackage</c>.
    /// </remarks>
    public class ToolchainLinuxX86_64 : SysrootPackage
    {
        /// <summary>
        /// UPM package name that ships the LLVM/Clang toolchain payload.
        /// </summary>
        private string _packageName => "com.unity.toolchain.linux-x86_64-linux";

        /// <summary>
        /// Human-readable package name as exposed to callers.
        /// </summary>
        public override string Name => _packageName;

        /// <summary>
        /// Host operating system this toolchain runs on.
        /// </summary>
        public override string HostPlatform => "linux";

        /// <summary>
        /// Host CPU architecture this toolchain runs on.
        /// </summary>
        public override string HostArch => "x86_64";

        /// <summary>
        /// Target operating system this toolchain is intended to produce binaries for.
        /// </summary>
        /// <remarks>
        /// The exact target triple and sysroot (if any) are provided elsewhere (e.g., by a sysroot package
        /// and compiler flags). This package focuses on supplying the host-side tool binaries.
        /// </remarks>
        public override string TargetPlatform => "linux";

        /// <summary>
        /// Identifier for the specific payload version expected by this package.
        /// </summary>
        /// <remarks>
        /// CI typically substitutes this placeholder at pack/publish time so the package resolves
        /// a precise, immutable payload directory on disk.
        /// </remarks>
        private string _payloadVersion => "15.0.6_99c1b61e5651af630294cdd18fbd4d55900aad70f94c602c76b35989f0b571cf-1";

        /// <summary>
        /// Relative path (under the cache) to the toolchain payload root for this host/target.
        /// </summary>
        private string _payloadDir;

        /// <summary>
        /// Relative path (inside the payload) to the linker binary to use with <c>-fuse-ld=</c>.
        /// </summary>
        private string _linkerFile => "bin/ld.lld";

        /// <summary>
        /// Initializes the package and registers its toolchain payload so it can be resolved on disk.
        /// </summary>
        public ToolchainLinuxX86_64()
            : base()
        {
            _payloadDir = $"llvm-linux-x64/{_payloadVersion}";
            RegisterPayload(_packageName, _payloadDir);
        }

        /// <summary>
        /// Gets the absolute path to the installed toolchain payload root directory.
        /// </summary>
        /// <returns>Absolute path to the toolchain payload root.</returns>
        public string PathToPayload()
        {
            return PayloadInstallDirectory(_payloadDir).ToString();
        }

        /// <summary>
        /// Absolute path to a sysroot for IL2CPP (not applicable for this toolchain-only package).
        /// </summary>
        /// <returns>Always <c>null</c> for this package.</returns>
        public override string GetSysrootPath()
        {
            return null;
        }

        /// <summary>
        /// Absolute path to the installed toolchain that IL2CPP should use.
        /// </summary>
        /// <returns>Absolute path to the toolchain payload root.</returns>
        public override string GetToolchainPath()
        {
            return PathToPayload();
        }

        /// <summary>
        /// Additional compiler flags to pass to IL2CPP/Clang (none required by this package).
        /// </summary>
        /// <remarks>
        /// Targeting flags (e.g., <c>-target</c>) are typically supplied by the corresponding sysroot package.
        /// </remarks>
        /// <returns><c>null</c> (no extra compiler flags).</returns>
        public override string GetIl2CppCompilerFlags()
        {
            return null;
        }

        /// <summary>
        /// Additional linker flags to ensure IL2CPP uses this package's linker.
        /// </summary>
        /// <remarks>
        /// Forces the linker to the payload’s <c>ld.lld</c> via <c>-fuse-ld=</c> (quoted path), and links
        /// libstdc++ statically for portability. Adjust if your distribution policy prefers dynamic libstdc++.
        /// </remarks>
        /// <returns>A space-separated string of linker flags.</returns>
        public override string GetIl2CppLinkerFlags()
        {
            var linkerpath = PayloadInstallDirectory(_payloadDir).Combine(_linkerFile);
            return $"-fuse-ld={linkerpath.InQuotes()} -static-libstdc++";
        }
    }
}
