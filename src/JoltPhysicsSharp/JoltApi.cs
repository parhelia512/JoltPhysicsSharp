// Copyright (c) Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using System.Numerics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

namespace JoltPhysicsSharp;

internal static unsafe partial class JoltApi
{
    private const DllImportSearchPath DefaultDllImportSearchPath = DllImportSearchPath.ApplicationDirectory | DllImportSearchPath.UserDirectories | DllImportSearchPath.UseDllDirectoryForDependencies;

    /// <summary>
    /// Raised whenever a native library is loaded by Jolt.
    /// Handlers can be added to this event to customize how libraries are loaded, and they will be used first whenever a new native library is being resolved.
    /// </summary>
    public static event DllImportResolver? JoltDllImporterResolver;

    private const string LibName = "joltc";
    private const string LibDoubleName = "joltc_double";

    public static bool DoublePrecision { get; set; }

    static JoltApi()
    {
        NativeLibrary.SetDllImportResolver(typeof(JoltApi).Assembly, OnDllImport);
    }

    private static IntPtr OnDllImport(string libraryName, Assembly assembly, DllImportSearchPath? searchPath)
    {
        if (libraryName != LibName)
        {
            return IntPtr.Zero;
        }

        IntPtr nativeLibrary = IntPtr.Zero;
        DllImportResolver? resolver = JoltDllImporterResolver;
        if (resolver != null)
        {
            nativeLibrary = resolver(libraryName, assembly, searchPath);
        }

        if (nativeLibrary != IntPtr.Zero)
        {
            return nativeLibrary;
        }

        if (OperatingSystem.IsWindows())
        {
            string dllName = DoublePrecision ? $"{LibDoubleName}.dll" : $"{LibName}.dll";

            if (NativeLibrary.TryLoad(dllName, assembly, searchPath, out nativeLibrary))
            {
                return nativeLibrary;
            }
        }
        else if (OperatingSystem.IsLinux())
        {
            string dllName = DoublePrecision ? $"lib{LibDoubleName}.so" : $"lib{LibName}.so";

            if (NativeLibrary.TryLoad(dllName, assembly, searchPath, out nativeLibrary))
            {
                return nativeLibrary;
            }
        }
        else if (OperatingSystem.IsMacOS() || OperatingSystem.IsMacCatalyst())
        {
            string dllName = DoublePrecision ? $"lib{LibDoubleName}.dylib" : $"lib{LibName}.dylib";

            if (NativeLibrary.TryLoad(dllName, assembly, searchPath, out nativeLibrary))
            {
                return nativeLibrary;
            }
        }

        string libraryLoadName = DoublePrecision ? $"lib{LibDoubleName}" : $"lib{LibName}";

        if (NativeLibrary.TryLoad(libraryLoadName, assembly, searchPath, out nativeLibrary))
        {
            return nativeLibrary;
        }

        libraryLoadName = DoublePrecision ? LibDoubleName : LibName;
        if (NativeLibrary.TryLoad(libraryLoadName, assembly, searchPath, out nativeLibrary))
        {
            return nativeLibrary;
        }

        return IntPtr.Zero;
    }

    /// <summary>Converts an unmanaged string to a managed version.</summary>
    /// <param name="unmanaged">The unmanaged string to convert.</param>
    /// <returns>A managed string.</returns>
    public static string? ConvertToManaged(byte* unmanaged)
    {
        if (unmanaged == null)
            return null;

        return UTF8EncodingRelaxed.Default.GetString(MemoryMarshal.CreateReadOnlySpanFromNullTerminated(unmanaged));
    }

    /// <summary>Converts an unmanaged string to a managed version.</summary>
    /// <param name="unmanaged">The unmanaged string to convert.</param>
    /// <returns>A managed string.</returns>
    public static string? ConvertToManaged(byte* unmanaged, int maxLength)
    {
        if (unmanaged == null)
            return null;

        var span = new ReadOnlySpan<byte>(unmanaged, maxLength);
        var indexOfZero = span.IndexOf((byte)0);
        return indexOfZero < 0 ? UTF8EncodingRelaxed.Default.GetString(span) : UTF8EncodingRelaxed.Default.GetString(span.Slice(0, indexOfZero));
    }


    [LibraryImport(LibName)]
    [return: MarshalAs(UnmanagedType.U1)]
    public static partial bool JPH_Init();

    [LibraryImport(LibName)]
    public static partial void JPH_Shutdown();

    [LibraryImport(LibName)]
    public static partial void JPH_SetTraceHandler(delegate* unmanaged<byte*, void> callback);

    [LibraryImport(LibName)]
    public static partial void JPH_SetAssertFailureHandler(delegate* unmanaged<byte*, byte*, byte*, uint, Bool8> callback);

    //  BroadPhaseLayerInterface
    [LibraryImport(LibName)]
    public static partial nint JPH_BroadPhaseLayerInterfaceMask_Create(uint numBroadPhaseLayers);

    [LibraryImport(LibName)]
    public static partial void JPH_BroadPhaseLayerInterfaceMask_ConfigureLayer(nint bpInterface, in BroadPhaseLayer broadPhaseLayer, uint groupsToInclude, uint groupsToExclude);


    [LibraryImport(LibName)]
    public static partial nint JPH_BroadPhaseLayerInterfaceTable_Create(uint numObjectLayers, uint numBroadPhaseLayers);

    [LibraryImport(LibName)]
    public static partial void JPH_BroadPhaseLayerInterfaceTable_MapObjectToBroadPhaseLayer(nint bpInterface, ObjectLayer objectLayer, BroadPhaseLayer broadPhaseLayer);

    //  ObjectVsBroadPhaseLayerFilter
    [LibraryImport(LibName)]
    public static partial nint JPH_ObjectVsBroadPhaseLayerFilterMask_Create(nint broadPhaseLayerInterface);

    [LibraryImport(LibName)]
    public static partial nint JPH_ObjectVsBroadPhaseLayerFilterTable_Create(nint broadPhaseLayerInterface, uint numBroadPhaseLayers, nint objectLayerPairFilter, uint numObjectLayers);

    [LibraryImport(LibName)]
    public static partial void JPH_ObjectVsBroadPhaseLayerFilter_Destroy(nint handle);

    #region JPH_ObjectLayerPairFilter
    [LibraryImport(LibName)]
    public static partial nint JPH_ObjectLayerPairFilterMask_Create();

    [LibraryImport(LibName)]
    public static partial ObjectLayer JPH_ObjectLayerPairFilterMask_GetObjectLayer(uint group, uint mask);

    [LibraryImport(LibName)]
    public static partial uint JPH_ObjectLayerPairFilterMask_GetGroup(in ObjectLayer layer);

    [LibraryImport(LibName)]
    public static partial uint JPH_ObjectLayerPairFilterMask_GetMask(in ObjectLayer layer);

    [LibraryImport(LibName)]
    public static partial nint JPH_ObjectLayerPairFilterTable_Create(uint numObjectLayers);

    [LibraryImport(LibName)]
    public static partial void JPH_ObjectLayerPairFilterTable_DisableCollision(nint objectFilter, ObjectLayer layer1, ObjectLayer layer2);

    [LibraryImport(LibName)]
    public static partial void JPH_ObjectLayerPairFilterTable_EnableCollision(nint objectFilter, ObjectLayer layer1, ObjectLayer layer2);
    #endregion

    //  ObjectLayerPairFilterTable


    //  BroadPhaseLayerFilter
    public struct JPH_BroadPhaseLayerFilter_Procs
    {
        public delegate* unmanaged<nint, BroadPhaseLayer, Bool8> ShouldCollide;
    }

    [LibraryImport(LibName)]
    public static partial void JPH_BroadPhaseLayerFilter_SetProcs(nint filter, JPH_BroadPhaseLayerFilter_Procs procs, nint userData);

    [LibraryImport(LibName)]
    public static partial nint JPH_BroadPhaseLayerFilter_Create();

    [LibraryImport(LibName)]
    public static partial void JPH_BroadPhaseLayerFilter_Destroy(nint handle);

    //  ObjectLayerFilter
    public struct JPH_ObjectLayerFilter_Procs
    {
        public delegate* unmanaged<IntPtr, ObjectLayer, Bool8> ShouldCollide;
    }

    [LibraryImport(LibName)]
    public static partial void JPH_ObjectLayerFilter_SetProcs(nint filter, JPH_ObjectLayerFilter_Procs procs, nint userData);

    [LibraryImport(LibName)]
    public static partial nint JPH_ObjectLayerFilter_Create();

    [LibraryImport(LibName)]
    public static partial void JPH_ObjectLayerFilter_Destroy(nint handle);

    //  BodyFilter
    public struct JPH_BodyFilter_Procs
    {
        public delegate* unmanaged<nint, BodyID, Bool8> ShouldCollide;
        public delegate* unmanaged<nint, IntPtr, Bool8> ShouldCollideLocked;
    }

    [LibraryImport(LibName)]
    public static partial void JPH_BodyFilter_SetProcs(nint filter, JPH_BodyFilter_Procs procs, nint userData);

    [LibraryImport(LibName)]
    public static partial nint JPH_BodyFilter_Create();

    [LibraryImport(LibName)]
    public static partial void JPH_BodyFilter_Destroy(nint handle);

    /* ShapeSettings */
    [LibraryImport(LibName)]
    public static partial void JPH_ShapeSettings_Destroy(nint settings);

    [LibraryImport(LibName)]
    public static partial ulong JPH_ShapeSettings_GetUserData(nint settings);

    [LibraryImport(LibName)]
    public static partial void JPH_ShapeSettings_SetUserData(nint settings, ulong userData);

    /* ConvexShape */

    [LibraryImport(LibName)]
    public static partial float JPH_ConvexShape_GetDensity(nint shape);

    [LibraryImport(LibName)]
    public static partial void JPH_ConvexShape_SetDensity(nint shape, float value);

    /* BoxShapeSettings */
    [LibraryImport(LibName)]
    public static partial nint JPH_BoxShapeSettings_Create(in Vector3 halfExtent, float convexRadius);

    [LibraryImport(LibName)]
    public static partial nint JPH_BoxShapeSettings_CreateShape(nint settings);

    [LibraryImport(LibName)]
    public static partial nint JPH_BoxShape_Create(in Vector3 halfExtent, float convexRadius);

    [LibraryImport(LibName)]
    public static partial void JPH_BoxShape_GetHalfExtent(IntPtr handle, out Vector3 halfExtent);

    [LibraryImport(LibName)]
    public static partial float JPH_BoxShape_GetConvexRadius(IntPtr handle);

    /* SphereShape */
    [LibraryImport(LibName)]
    public static partial nint JPH_SphereShapeSettings_Create(float radius);

    [LibraryImport(LibName)]
    public static partial nint JPH_SphereShapeSettings_CreateShape(nint settings);

    [LibraryImport(LibName)]
    public static partial float JPH_SphereShapeSettings_GetRadius(nint shape);

    [LibraryImport(LibName)]
    public static partial void JPH_SphereShapeSettings_SetRadius(nint shape, float radius);

    /* PlaneShape */
    [LibraryImport(LibName)]
    public static partial nint JPH_PlaneShapeSettings_Create(in Plane plane, nint material, float halfExtent);

    [LibraryImport(LibName)]
    public static partial nint JPH_PlaneShapeSettings_CreateShape(nint settings);

    [LibraryImport(LibName)]
    public static partial nint JPH_PlaneShape_Create(in Plane plane, nint material, float halfExtent);

    [LibraryImport(LibName)]
    public static partial void JPH_PlaneShape_GetPlane(nint handle, out Plane result);

    [LibraryImport(LibName)]
    public static partial float JPH_PlaneShape_GetHalfExtent(nint handle);

    /* TriangleShape */
    [LibraryImport(LibName)]
    public static partial nint JPH_TriangleShapeSettings_Create(in Vector3 v1, in Vector3 v2, in Vector3 v3, float convexRadius);

    [LibraryImport(LibName)]
    public static partial nint JPH_TriangleShapeSettings_CreateShape(nint settings);

    [LibraryImport(LibName)]
    public static partial nint JPH_TriangleShape_Create(in Vector3 v1, in Vector3 v2, in Vector3 v3, float convexRadius);

    [LibraryImport(LibName)]
    public static partial float JPH_TriangleShape_GetConvexRadius(nint shape);

    [LibraryImport(LibName)]
    public static partial void JPH_TriangleShape_GetVertex1(nint handle, Vector3* result);

    [LibraryImport(LibName)]
    public static partial void JPH_TriangleShape_GetVertex2(nint handle, Vector3* result);

    [LibraryImport(LibName)]
    public static partial void JPH_TriangleShape_GetVertex3(nint handle, Vector3* result);

    /* CapsuleShape */
    [LibraryImport(LibName)]
    public static partial nint JPH_CapsuleShapeSettings_Create(float halfHeightOfCylinder, float radius);

    [LibraryImport(LibName)]
    public static partial nint JPH_CapsuleShapeSettings_CreateShape(nint settings);

    [LibraryImport(LibName)]
    public static partial nint JPH_CapsuleShape_Create(float halfHeightOfCylinder, float radius);

    [LibraryImport(LibName)]
    public static partial float JPH_CapsuleShape_GetRadius(nint handle);

    [LibraryImport(LibName)]
    public static partial float JPH_CapsuleShape_GetHalfHeightOfCylinder(nint handle);

    /* CylinderShape */
    [LibraryImport(LibName)]
    public static partial nint JPH_CylinderShapeSettings_Create(float halfHeight, float radius, float convexRadius);

    [LibraryImport(LibName)]
    public static partial nint JPH_CylinderShapeSettings_CreateShape(nint settings);

    [LibraryImport(LibName)]
    public static partial nint JPH_CylinderShape_Create(float halfHeight, float radius);

    [LibraryImport(LibName)]
    public static partial float JPH_CylinderShape_GetRadius(nint handle);

    [LibraryImport(LibName)]
    public static partial float JPH_CylinderShape_GetHalfHeight(nint handle);

    /* TaperedCylinderShape */
    [LibraryImport(LibName)]
    public static partial nint JPH_TaperedCylinderShapeSettings_Create(float halfHeightOfTaperedCylinder, float topRadius, float bottomRadius, float convexRadius, nint material);

    [LibraryImport(LibName)]
    public static partial nint JPH_TaperedCylinderShapeSettings_CreateShape(nint settings);

    [LibraryImport(LibName)]
    public static partial float JPH_TaperedCylinderShape_GetTopRadius(nint shape);

    [LibraryImport(LibName)]
    public static partial float JPH_TaperedCylinderShape_GetBottomRadius(nint shape);

    [LibraryImport(LibName)]
    public static partial float JPH_TaperedCylinderShape_GetConvexRadius(nint shape);

    [LibraryImport(LibName)]
    public static partial float JPH_TaperedCylinderShape_GetHalfHeight(nint shape);

    /* ConvexHullShape */
    [LibraryImport(LibName)]
    public static partial nint JPH_ConvexHullShapeSettings_Create(Vector3* points, int pointsCount, float maxConvexRadius);

    [LibraryImport(LibName)]
    public static partial nint JPH_ConvexHullShapeSettings_CreateShape(nint settings);

    [LibraryImport(LibName)]
    public static partial uint JPH_ConvexHullShape_GetNumPoints(nint shape);

    [LibraryImport(LibName)]
    public static partial void JPH_ConvexHullShape_GetPoint(nint shape, uint index, Vector3* result);

    [LibraryImport(LibName)]
    public static partial uint JPH_ConvexHullShape_GetNumFaces(nint shape);

    [LibraryImport(LibName)]
    public static partial uint JPH_ConvexHullShape_GetFaceVertices(nint shape, uint faceIndex, uint maxVertices, uint* vertices);

    [LibraryImport(LibName)]
    public static partial uint JPH_ConvexHullShape_GetNumVerticesInFace(nint shape, uint faceIndex);

    /* ConvexShape */
    [LibraryImport(LibName)]
    public static partial float JPH_ConvexShapeSettings_GetDensity(nint shape);

    [LibraryImport(LibName)]
    public static partial void JPH_ConvexShapeSettings_SetDensity(nint shape, float value);

    /* MeshShape  */
    [LibraryImport(LibName)]
    public static partial nint JPH_MeshShapeSettings_Create(Triangle* triangle, int triangleCount);

    [LibraryImport(LibName)]
    public static partial nint JPH_MeshShapeSettings_Create2(Vector3* vertices, int verticesCount, IndexedTriangle* triangles, int triangleCount);

    [LibraryImport(LibName)]
    public static partial void JPH_MeshShapeSettings_Sanitize(IntPtr shape);

    [LibraryImport(LibName)]
    public static partial nint JPH_MeshShapeSettings_CreateShape(nint settings);

    /* HeightFieldShape  */
    [LibraryImport(LibName)]
    public static partial nint JPH_HeightFieldShapeSettings_Create(float* samples, in Vector3 offset, in Vector3 scale, int sampleCount);

    [LibraryImport(LibName)]
    public static partial nint JPH_HeightFieldShapeSettings_CreateShape(nint settings);

    [LibraryImport(LibName)]
    public static partial void JPH_HeightFieldShapeSettings_DetermineMinAndMaxSample(nint settings, out float outMinValue, out float outMaxValue, out float outQuantizationScale);

    [LibraryImport(LibName)]
    public static partial uint JPH_HeightFieldShapeSettings_CalculateBitsPerSampleForError(nint settings, float maxError);

    [LibraryImport(LibName)]
    public static partial uint JPH_HeightFieldShape_GetSampleCount(nint shape);
    [LibraryImport(LibName)]
    public static partial uint JPH_HeightFieldShape_GetBlockSize(nint shape);
    [LibraryImport(LibName)]
    public static partial /*JPH_PhysicsMaterial**/nint JPH_HeightFieldShape_GetMaterial(nint shape, uint x, uint y);
    [LibraryImport(LibName)]
    public static partial void JPH_HeightFieldShape_GetPosition(nint shape, uint x, uint y, out Vector3 result);
    [LibraryImport(LibName)]
    [return: MarshalAs(UnmanagedType.U1)]
    public static partial bool JPH_HeightFieldShape_IsNoCollision(nint shape, uint x, uint y);
    [LibraryImport(LibName)]
    [return: MarshalAs(UnmanagedType.U1)]
    public static partial bool JPH_HeightFieldShape_ProjectOntoSurface(nint shape, in Vector3 localPosition, out Vector3 surfacePosition, out SubShapeID subShapeID);
    [LibraryImport(LibName)]
    public static partial float JPH_HeightFieldShape_GetMinHeightValue(nint shape);
    [LibraryImport(LibName)]
    public static partial float JPH_HeightFieldShape_GetMaxHeightValue(nint shape);

    /* JPH_TaperedCapsuleShape */
    [LibraryImport(LibName)]
    public static partial nint JPH_TaperedCapsuleShapeSettings_Create(float halfHeightOfTaperedCylinder, float topRadius, float bottomRadius);

    [LibraryImport(LibName)]
    public static partial nint JPH_TaperedCapsuleShapeSettings_CreateShape(nint settings);

    [LibraryImport(LibName)]
    public static partial float JPH_TaperedCapsuleShape_GetTopRadius(nint shape);

    [LibraryImport(LibName)]
    public static partial float JPH_TaperedCapsuleShape_GetBottomRadius(nint shape);

    [LibraryImport(LibName)]
    public static partial float JPH_TaperedCapsuleShape_GetHalfHeight(nint shape);

    /* CompoundShape */
    [LibraryImport(LibName)]
    public static partial void JPH_CompoundShapeSettings_AddShape(nint handle, Vector3* position, Quaternion* rotation, nint shapeSettings, uint userData);

    [LibraryImport(LibName)]
    public static partial void JPH_CompoundShapeSettings_AddShape2(nint handle, Vector3* position, Quaternion* rotation, nint shape, uint userData);

    [LibraryImport(LibName)]
    public static partial uint JPH_CompoundShape_GetNumSubShapes(nint handle);

    [LibraryImport(LibName)]
    public static partial nint JPH_StaticCompoundShapeSettings_Create();

    [LibraryImport(LibName)]
    public static partial nint JPH_StaticCompoundShape_Create(nint settings);

    /* MutableCompoundShape */
    [LibraryImport(LibName)]
    public static partial nint JPH_MutableCompoundShapeSettings_Create();

    [LibraryImport(LibName)]
    public static partial nint JPH_MutableCompoundShape_Create(nint settings);

    /* RotatedTranslatedShape */
    [LibraryImport(LibName)]
    public static partial nint JPH_RotatedTranslatedShapeSettings_Create(Vector3* position, Quaternion* rotation, nint shapeSettings);

    [LibraryImport(LibName)]
    public static partial nint JPH_RotatedTranslatedShapeSettings_Create2(Vector3* position, Quaternion* rotation, nint shape);

    [LibraryImport(LibName)]
    public static partial nint JPH_RotatedTranslatedShapeSettings_CreateShape(nint settings);

    [LibraryImport(LibName)]
    public static partial nint JPH_RotatedTranslatedShape_Create(Vector3* position, Quaternion* rotation, nint shape);

    [LibraryImport(LibName)]
    public static partial nint JPH_RotatedTranslatedShape_GetPosition(nint shape, Vector3* position);

    [LibraryImport(LibName)]
    public static partial nint JPH_RotatedTranslatedShape_GetRotation(nint shape, Quaternion* rotation);

    /* JPH_OffsetCenterOfMassShape */
    [LibraryImport(LibName)]
    public static partial nint JPH_OffsetCenterOfMassShapeSettings_Create(in Vector3 offset, nint shapeSettings);

    [LibraryImport(LibName)]
    public static partial nint JPH_OffsetCenterOfMassShapeSettings_Create2(in Vector3 offset, nint shape);

    [LibraryImport(LibName)]
    public static partial nint JPH_OffsetCenterOfMassShapeSettings_CreateShape(nint settings);

    [LibraryImport(LibName)]
    public static partial nint JPH_OffsetCenterOfMassShape_Create(in Vector3 offset, nint shape);

    [LibraryImport(LibName)]
    public static partial void JPH_OffsetCenterOfMassShape_GetOffset(nint handle, out Vector3 offset);

    /* Shape */
    [LibraryImport(LibName)]
    public static partial void JPH_Shape_Destroy(nint shape);

    [LibraryImport(LibName)]
    public static partial ShapeType JPH_Shape_GetType(nint shape);

    [LibraryImport(LibName)]
    public static partial ShapeSubType JPH_Shape_GetSubType(nint shape);

    [LibraryImport(LibName)]
    public static partial ulong JPH_Shape_GetUserData(nint shape);

    [LibraryImport(LibName)]
    public static partial void JPH_Shape_SetUserData(nint shape, ulong userData);

    [LibraryImport(LibName)]
    [return: MarshalAs(UnmanagedType.U1)]
    public static partial bool JPH_Shape_MustBeStatic(nint shape);

    [LibraryImport(LibName)]
    public static partial void JPH_Shape_GetCenterOfMass(nint handle, Vector3* result);

    [LibraryImport(LibName)]
    public static partial void JPH_Shape_GetLocalBounds(nint shape, BoundingBox* box);

    [LibraryImport(LibName)]
    public static partial uint JPH_Shape_GetSubShapeIDBitsRecursive(nint shape);

    [LibraryImport(LibName)]
    public static partial float JPH_Shape_GetInnerRadius(nint handle);

    [LibraryImport(LibName)]
    public static partial float JPH_Shape_GetVolume(nint handle);

    [LibraryImport(LibName)]
    public static partial void JPH_Shape_GetMassProperties(nint shape, MassProperties* properties);

    [LibraryImport(LibName)]
    public static partial void JPH_Shape_GetWorldSpaceBounds(nint shape, Matrix4x4* centerOfMassTransform, Vector3* scale, BoundingBox* result);

    [LibraryImport(LibName, EntryPoint = nameof(JPH_Shape_GetWorldSpaceBounds))]
    public static partial void JPH_Shape_GetWorldSpaceBoundsDouble(nint shape, RMatrix4x4* centerOfMassTransform, Vector3* scale, BoundingBox* result);

    [LibraryImport(LibName)]
    public static partial nint JPH_Shape_GetMaterial(nint shape, SubShapeID subShapeID);

    [LibraryImport(LibName)]
    public static partial void JPH_Shape_GetSurfaceNormal(nint shape, SubShapeID subShapeID, Vector3* localPosition, Vector3* normal);

    [LibraryImport(LibName)]
    [return: MarshalAs(UnmanagedType.U1)]
    public static partial bool JPH_Shape_CastRay(nint shape, in Vector3 origin, in Vector3 direction, out RayCastResult hit);

    [LibraryImport(LibName)]
    [return: MarshalAs(UnmanagedType.U1)]
    public static partial bool JPH_Shape_CollidePoint(nint shape, in Vector3 point);

    /* SphereShape */
    [LibraryImport(LibName)]
    public static partial nint JPH_SphereShape_Create(float radius);

    [LibraryImport(LibName)]
    public static partial float JPH_SphereShape_GetRadius(nint shape);

    /* BodyCreationSettings */
    [LibraryImport(LibName)]
    public static partial nint JPH_BodyCreationSettings_Create();

    [LibraryImport(LibName, EntryPoint = nameof(JPH_BodyCreationSettings_Create2))]
    public static partial nint JPH_BodyCreationSettings_Create2(nint shapeSettings, Vector3* position, Quaternion* rotation, MotionType motionType, ushort objectLayer);

    [LibraryImport(LibName, EntryPoint = nameof(JPH_BodyCreationSettings_Create2))]
    public static partial nint JPH_BodyCreationSettings_Create2Double(nint shapeSettings, Double3* position, Quaternion* rotation, MotionType motionType, ushort objectLayer);

    [LibraryImport(LibName, EntryPoint = nameof(JPH_BodyCreationSettings_Create3))]
    public static partial nint JPH_BodyCreationSettings_Create3(nint shape, Vector3* position, Quaternion* rotation, MotionType motionType, ushort objectLayer);

    [LibraryImport(LibName, EntryPoint = nameof(JPH_BodyCreationSettings_Create3))]
    public static partial nint JPH_BodyCreationSettings_Create3Double(nint shape, Double3* position, Quaternion* rotation, MotionType motionType, ushort objectLayer);

    [LibraryImport(LibName)]
    public static partial void JPH_BodyCreationSettings_Destroy(nint settings);

    [LibraryImport(LibName)]
    public static partial void JPH_BodyCreationSettings_GetPosition(nint settings, Vector3* velocity);

    [LibraryImport(LibName)]
    public static partial void JPH_BodyCreationSettings_SetPosition(nint settings, Vector3* velocity);

    [LibraryImport(LibName)]
    public static partial void JPH_BodyCreationSettings_GetRotation(nint settings, Quaternion* velocity);

    [LibraryImport(LibName)]
    public static partial void JPH_BodyCreationSettings_SetRotation(nint settings, Quaternion* velocity);

    [LibraryImport(LibName)]
    public static partial void JPH_BodyCreationSettings_GetLinearVelocity(nint settings, Vector3* velocity);

    [LibraryImport(LibName)]
    public static partial void JPH_BodyCreationSettings_SetLinearVelocity(nint settings, Vector3* velocity);

    [LibraryImport(LibName)]
    public static partial void JPH_BodyCreationSettings_GetAngularVelocity(nint settings, Vector3* velocity);

    [LibraryImport(LibName)]
    public static partial void JPH_BodyCreationSettings_SetAngularVelocity(nint settings, Vector3* velocity);

    [LibraryImport(LibName)]
    public static partial MotionType JPH_BodyCreationSettings_GetMotionType(nint settings);

    [LibraryImport(LibName)]
    public static partial void JPH_BodyCreationSettings_SetMotionType(nint settings, MotionType value);

    [LibraryImport(LibName)]
    public static partial AllowedDOFs JPH_BodyCreationSettings_GetAllowedDOFs(nint settings);

    [LibraryImport(LibName)]
    public static partial void JPH_BodyCreationSettings_SetAllowedDOFs(nint settings, AllowedDOFs value);

    /* SoftBodyCreationSettings */
    [LibraryImport(LibName)]
    public static partial nint JPH_SoftBodyCreationSettings_Create();

    [LibraryImport(LibName)]
    public static partial void JPH_SoftBodyCreationSettings_Destroy(nint settings);

    #region JPH_Constraint
    /* JPH_Constraint */
    [LibraryImport(LibName)]
    public static partial void JPH_ConstraintSettings_Destroy(nint handle);

    [LibraryImport(LibName)]
    [return: MarshalAs(UnmanagedType.U1)]
    public static partial bool JPH_ConstraintSettings_GetEnabled(nint handle);

    [LibraryImport(LibName)]
    public static partial void JPH_FixedConstraintSettings_SetEnabled(nint handle, [MarshalAs(UnmanagedType.U1)] bool value);

    [LibraryImport(LibName)]
    public static partial uint JPH_ConstraintSettings_GetConstraintPriority(nint handle);

    [LibraryImport(LibName)]
    public static partial void JPH_FixedConstraintSettings_SetConstraintPriority(nint handle, uint value);

    [LibraryImport(LibName)]
    public static partial uint JPH_ConstraintSettings_GetNumVelocityStepsOverride(nint handle);

    [LibraryImport(LibName)]
    public static partial void JPH_ConstraintSettings_SetNumVelocityStepsOverride(nint handle, uint value);

    [LibraryImport(LibName)]
    public static partial uint JPH_ConstraintSettings_GetNumPositionStepsOverride(nint handle);

    [LibraryImport(LibName)]
    public static partial void JPH_ConstraintSettings_SetNumPositionStepsOverride(nint handle, uint value);

    [LibraryImport(LibName)]
    public static partial float JPH_ConstraintSettings_GetDrawConstraintSize(nint handle);

    [LibraryImport(LibName)]
    public static partial void JPH_ConstraintSettings_SetDrawConstraintSize(nint handle, float value);


    [LibraryImport(LibName)]
    public static partial ulong JPH_ConstraintSettings_GetUserData(nint handle);

    [LibraryImport(LibName)]
    public static partial void JPH_ConstraintSettings_SetUserData(nint handle, ulong value);


    [LibraryImport(LibName)]
    public static partial void JPH_Constraint_Destroy(nint constraint);

    [LibraryImport(LibName)]
    public static partial nint JPH_Constraint_GetConstraintSettings(nint constraint);

    [LibraryImport(LibName)]
    public static partial ConstraintType JPH_Constraint_GetType(nint constraint);

    [LibraryImport(LibName)]
    public static partial ConstraintSubType JPH_Constraint_GetSubType(nint constraint);

    [LibraryImport(LibName)]
    public static partial uint JPH_Constraint_GetConstraintPriority(nint handle);

    [LibraryImport(LibName)]
    public static partial void JPH_Constraint_SetConstraintPriority(nint handle, uint value);

    [LibraryImport(LibName)]
    [return: MarshalAs(UnmanagedType.U1)]
    public static partial bool JPH_Constraint_GetEnabled(nint constraint);

    [LibraryImport(LibName)]
    public static partial void JPH_Constraint_SetEnabled(nint constraint, [MarshalAs(UnmanagedType.U1)] bool value);

    [LibraryImport(LibName)]
    public static partial ulong JPH_Constraint_GetUserData(nint constraint);

    [LibraryImport(LibName)]
    public static partial void JPH_Constraint_SetUserData(nint constraint, ulong value);

    [LibraryImport(LibName)]
    public static partial void JPH_Constraint_NotifyShapeChanged(nint constraint, uint bodyID, Vector3* deltaCOM);
    #endregion

    #region JPH_TwoBodyConstraint
    [LibraryImport(LibName)]
    public static partial nint JPH_TwoBodyConstraint_GetBody1(nint constraint);

    [LibraryImport(LibName)]
    public static partial nint JPH_TwoBodyConstraint_GetBody2(nint constraint);

    [LibraryImport(LibName)]
    public static partial void JPH_TwoBodyConstraint_GetConstraintToBody1Matrix(nint constraint, Matrix4x4* result);

    [LibraryImport(LibName)]
    public static partial void JPH_TwoBodyConstraint_GetConstraintToBody2Matrix(nint constraint, Matrix4x4* result);
    #endregion

    #region JPH_FixedConstraint
    [LibraryImport(LibName)]
    public static partial nint JPH_FixedConstraintSettings_Create();

    [LibraryImport(LibName)]
    public static partial ConstraintSpace JPH_FixedConstraintSettings_GetSpace(nint handle);

    [LibraryImport(LibName)]
    public static partial void JPH_FixedConstraintSettings_SetSpace(nint handle, ConstraintSpace value);

    [LibraryImport(LibName)]
    [return: MarshalAs(UnmanagedType.U1)]
    public static partial bool JPH_FixedConstraintSettings_GetAutoDetectPoint(nint handle);

    [LibraryImport(LibName)]
    public static partial void JPH_FixedConstraintSettings_SetAutoDetectPoint(nint handle, [MarshalAs(UnmanagedType.U1)] bool value);

    [LibraryImport(LibName)]
    public static partial void JPH_FixedConstraintSettings_GetPoint1(nint handle, Vector3* result); // RVec3

    [LibraryImport(LibName)]
    public static partial void JPH_FixedConstraintSettings_SetPoint1(nint handle, Vector3* value); // RVec3

    [LibraryImport(LibName)]
    public static partial void JPH_FixedConstraintSettings_GetAxisX1(nint handle, Vector3* result);

    [LibraryImport(LibName)]
    public static partial void JPH_FixedConstraintSettings_SetAxisX1(nint handle, Vector3* value);

    [LibraryImport(LibName)]
    public static partial void JPH_FixedConstraintSettings_GetAxisY1(nint handle, Vector3* result);

    [LibraryImport(LibName)]
    public static partial void JPH_FixedConstraintSettings_SetAxisY1(nint handle, Vector3* value);

    [LibraryImport(LibName)]
    public static partial void JPH_FixedConstraintSettings_GetPoint2(nint handle, Vector3* result); // RVec3

    [LibraryImport(LibName)]
    public static partial void JPH_FixedConstraintSettings_SetPoint2(nint handle, Vector3* value); // RVec3

    [LibraryImport(LibName)]
    public static partial void JPH_FixedConstraintSettings_GetAxisX2(nint handle, Vector3* result);

    [LibraryImport(LibName)]
    public static partial void JPH_FixedConstraintSettings_SetAxisX2(nint handle, Vector3* value);

    [LibraryImport(LibName)]
    public static partial void JPH_FixedConstraintSettings_GetAxisY2(nint handle, Vector3* result);

    [LibraryImport(LibName)]
    public static partial void JPH_FixedConstraintSettings_SetAxisY2(nint handle, Vector3* value);

    [LibraryImport(LibName)]
    public static partial nint JPH_FixedConstraintSettings_CreateConstraint(nint handle, nint body1, nint body2);

    [LibraryImport(LibName)]
    public static partial void JPH_FixedConstraint_GetTotalLambdaPosition(nint handle, Vector3* result);

    [LibraryImport(LibName)]
    public static partial void JPH_FixedConstraint_GetTotalLambdaRotation(nint handle, Vector3* result);
    #endregion

    #region JPH_DistanceConstraint
    /* JPH_DistanceConstraint */
    [LibraryImport(LibName)]
    public static partial nint JPH_DistanceConstraintSettings_Create();

    [LibraryImport(LibName)]
    public static partial ConstraintSpace JPH_DistanceConstraintSettings_GetSpace(nint handle);

    [LibraryImport(LibName)]
    public static partial void JPH_DistanceConstraintSettings_SetSpace(nint handle, ConstraintSpace value);

    [LibraryImport(LibName)]
    public static partial void JPH_DistanceConstraintSettings_GetPoint1(nint handle, out Vector3 result); // RVec3

    [LibraryImport(LibName)]
    public static partial void JPH_DistanceConstraintSettings_SetPoint1(nint handle, in Vector3 value); // RVec3

    [LibraryImport(LibName)]
    public static partial void JPH_DistanceConstraintSettings_GetPoint2(nint handle, out Vector3 result); // RVec3

    [LibraryImport(LibName)]
    public static partial void JPH_DistanceConstraintSettings_SetPoint2(nint handle, in Vector3 value); // RVec3

    [LibraryImport(LibName)]
    public static partial nint JPH_DistanceConstraintSettings_CreateConstraint(nint handle, nint body1, nint body2);

    [LibraryImport(LibName)]
    public static partial nint JPH_DistanceConstraint_SetDistance(nint constraint, float minDistance, float maxDistance);

    [LibraryImport(LibName)]
    public static partial float JPH_DistanceConstraint_GetMinDistance(nint constraint);

    [LibraryImport(LibName)]
    public static partial float JPH_DistanceConstraint_GetMaxDistance(nint constraint);
    [LibraryImport(LibName)]
    public static partial void JPH_DistanceConstraint_GetLimitsSpringSettings(nint constraint, SpringSettings* result);
    [LibraryImport(LibName)]
    public static partial void JPH_DistanceConstraint_SetLimitsSpringSettings(nint constraint, SpringSettings* settings);
    [LibraryImport(LibName)]
    public static partial float JPH_DistanceConstraint_GetTotalLambdaPosition(nint constraint);
    #endregion

    #region JPH_PointConstraint
    [LibraryImport(LibName)]
    public static partial nint JPH_PointConstraintSettings_Create();

    [LibraryImport(LibName)]
    public static partial ConstraintSpace JPH_PointConstraintSettings_GetSpace(nint handle);

    [LibraryImport(LibName)]
    public static partial void JPH_PointConstraintSettings_SetSpace(nint handle, ConstraintSpace value);

    [LibraryImport(LibName)]
    public static partial void JPH_PointConstraintSettings_GetPoint1(nint handle, Vector3* result); // RVec3

    [LibraryImport(LibName)]
    public static partial void JPH_PointConstraintSettings_SetPoint1(nint handle, Vector3* value); // RVec3

    [LibraryImport(LibName)]
    public static partial void JPH_PointConstraintSettings_GetPoint2(nint handle, Vector3* result); // RVec3

    [LibraryImport(LibName)]
    public static partial void JPH_PointConstraintSettings_SetPoint2(nint handle, Vector3* value); // RVec3

    [LibraryImport(LibName)]
    public static partial nint JPH_PointConstraintSettings_CreateConstraint(nint handle, nint body1, nint body2);

    [LibraryImport(LibName)]
    public static partial void JPH_PointConstraint_SetPoint1(nint handle, ConstraintSpace space, Vector3* value); // RVec3

    [LibraryImport(LibName)]
    public static partial void JPH_PointConstraint_SetPoint2(nint handle, ConstraintSpace space, Vector3* value); // RVec3

    [LibraryImport(LibName)]
    public static partial void JPH_PointConstraint_GetTotalLambdaPosition(nint handle, Vector3* result);
    #endregion

    #region JPH_HingeConstraint
    [LibraryImport(LibName)]
    public static partial nint JPH_HingeConstraintSettings_Create();

    [LibraryImport(LibName)]
    public static partial void JPH_HingeConstraintSettings_GetPoint1(IntPtr handle, out Vector3 result); // RVec3

    [LibraryImport(LibName)]
    public static partial void JPH_HingeConstraintSettings_SetPoint1(IntPtr handle, in Vector3 value); // RVec3

    [LibraryImport(LibName)]
    public static partial void JPH_HingeConstraintSettings_GetPoint2(IntPtr handle, out Vector3 result); // RVec3

    [LibraryImport(LibName)]
    public static partial void JPH_HingeConstraintSettings_SetPoint2(IntPtr handle, in Vector3 value); // RVec3

    [LibraryImport(LibName)]
    public static partial nint JPH_HingeConstraintSettings_CreateConstraint(nint handle, nint body1, nint body2);
    #endregion

    #region JPH_SliderConstraint
    [LibraryImport(LibName)]
    public static partial nint JPH_SliderConstraintSettings_Create();

    [LibraryImport(LibName)]
    public static partial void JPH_SliderConstraintSettings_SetSliderAxis(IntPtr handle, Vector3* value);

    [LibraryImport(LibName)]
    public static partial nint JPH_SliderConstraintSettings_CreateConstraint(nint handle, nint body1, nint body2);
    #endregion

    #region JPH_SwingTwistConstraint
    [LibraryImport(LibName)]
    public static partial nint JPH_SwingTwistConstraintSettings_Create();

    [LibraryImport(LibName)]
    public static partial nint JPH_SwingTwistConstraintSettings_CreateConstraint(nint handle, nint body1, nint body2);

    [LibraryImport(LibName)]
    public static partial float JPH_SwingTwistConstraint_GetNormalHalfConeAngle(nint handle);
    #endregion

    #region JPH_SixDOFConstraint
    [LibraryImport(LibName)]
    public static partial nint JPH_SixDOFConstraintSettings_Create();

    [LibraryImport(LibName)]
    public static partial nint JPH_SixDOFConstraintSettings_CreateConstraint(nint handle, nint body1, nint body2);

    [LibraryImport(LibName)]
    public static partial float JPH_SixDOFConstraint_GetLimitsMin(nint handle, uint axis);

    [LibraryImport(LibName)]
    public static partial float JPH_SixDOFConstraint_GetLimitsMax(nint handle, uint axis);
    #endregion

    #region JPH_ConeConstraint
    [LibraryImport(LibName)]
    public static partial nint JPH_ConeConstraintSettings_Create();
    [LibraryImport(LibName)]
    public static partial void JPH_ConeConstraintSettings_GetPoint1(nint settings, Vector3* result); // RVec3
    [LibraryImport(LibName)]
    public static partial void JPH_ConeConstraintSettings_SetPoint1(nint settings, Vector3* value); // RVec3
    [LibraryImport(LibName)]
    public static partial void JPH_ConeConstraintSettings_GetPoint2(nint settings, Vector3* result); // RVec3
    [LibraryImport(LibName)]
    public static partial void JPH_ConeConstraintSettings_SetPoint2(nint settings, Vector3* value); // RVec3
    [LibraryImport(LibName)]
    public static partial void JPH_ConeConstraintSettings_SetTwistAxis1(nint settings, Vector3* value);
    [LibraryImport(LibName)]
    public static partial void JPH_ConeConstraintSettings_GetTwistAxis1(nint settings, Vector3* result);
    [LibraryImport(LibName)]
    public static partial void JPH_ConeConstraintSettings_SetTwistAxis2(nint settings, Vector3* value);
    [LibraryImport(LibName)]
    public static partial void JPH_ConeConstraintSettings_GetTwistAxis2(nint settings, Vector3* result);
    [LibraryImport(LibName)]
    public static partial void JPH_ConeConstraintSettings_SetHalfConeAngle(nint settings, float halfConeAngle);
    [LibraryImport(LibName)]
    public static partial float JPH_ConeConstraintSettings_GetHalfConeAngle(nint settings);
    [LibraryImport(LibName)]
    public static partial nint JPH_ConeConstraintSettings_CreateConstraint(nint settings, nint body1, nint body2);

    [LibraryImport(LibName)]
    public static partial void JPH_ConeConstraint_SetHalfConeAngle(nint constraint, float halfConeAngle);
    [LibraryImport(LibName)]
    public static partial float JPH_ConeConstraint_GetCosHalfConeAngle(nint constraint);
    [LibraryImport(LibName)]
    public static partial void JPH_ConeConstraint_GetTotalLambdaPosition(nint constraint, Vector3* result);
    [LibraryImport(LibName)]
    public static partial float JPH_ConeConstraint_GetTotalLambdaRotation(nint constraint);
    #endregion

    /* PhysicsSystem */
    [StructLayout(LayoutKind.Sequential)]
    public struct NativePhysicsSystemSettings
    {
        public int maxBodies; /* 10240 */
        public int numBodyMutexes; /* 0 */
        public int maxBodyPairs; /* 65536 */
        public int maxContactConstraints; /* 10240 */
        private int _padding;
        public /*BroadPhaseLayerInterfaceTable*/ nint broadPhaseLayerInterface;
        public /*ObjectLayerPairFilterTable**/ nint objectLayerPairFilter;
        public /*ObjectVsBroadPhaseLayerFilterTable* */nint objectVsBroadPhaseLayerFilter;
    }

    [LibraryImport(LibName)]
    public static partial nint JPH_PhysicsSystem_Create(NativePhysicsSystemSettings* settings);

    [LibraryImport(LibName)]
    public static partial void JPH_PhysicsSystem_Destroy(nint handle);

    [LibraryImport(LibName)]
    public static partial void JPH_PhysicsSystem_GetPhysicsSettings(nint handle, PhysicsSettings* result);

    [LibraryImport(LibName)]
    public static partial void JPH_PhysicsSystem_SetPhysicsSettings(nint handle, PhysicsSettings* result);

    [LibraryImport(LibName)]
    public static partial void JPH_PhysicsSystem_OptimizeBroadPhase(nint handle);

    [LibraryImport(LibName)]
    public static partial PhysicsUpdateError JPH_PhysicsSystem_Update(nint handle, float deltaTime, int collisionSteps);

    [LibraryImport(LibName)]
    public static partial void JPH_PhysicsSystem_SetContactListener(IntPtr system, IntPtr listener);

    [LibraryImport(LibName)]
    public static partial void JPH_PhysicsSystem_SetBodyActivationListener(IntPtr system, IntPtr listener);

    [LibraryImport(LibName)]
    public static partial uint JPH_PhysicsSystem_GetNumBodies(IntPtr system);

    [LibraryImport(LibName)]
    public static partial uint JPH_PhysicsSystem_GetNumActiveBodies(IntPtr system, BodyType type);

    [LibraryImport(LibName)]
    public static partial uint JPH_PhysicsSystem_GetMaxBodies(nint system);

    [LibraryImport(LibName)]
    public static partial uint JPH_PhysicsSystem_GetNumConstraints(nint system);

    [LibraryImport(LibName)]
    public static partial void JPH_PhysicsSystem_GetGravity(nint handle, out Vector3 velocity);

    [LibraryImport(LibName)]
    public static partial void JPH_PhysicsSystem_SetGravity(nint handle, in Vector3 velocity);

    [LibraryImport(LibName)]
    public static partial void JPH_PhysicsSystem_AddConstraint(nint handle, IntPtr constraint);

    [LibraryImport(LibName)]
    public static partial void JPH_PhysicsSystem_RemoveConstraint(nint handle, IntPtr constraint);

    [LibraryImport(LibName)]
    public static partial void JPH_PhysicsSystem_AddConstraints(nint handle, nint* constraints, uint count);

    [LibraryImport(LibName)]
    public static partial void JPH_PhysicsSystem_RemoveConstraints(nint handle, nint* constraints, uint count);

    [LibraryImport(LibName)]
    public static partial void JPH_PhysicsSystem_GetBodies(nint handle, BodyID* ids, uint count);

    [LibraryImport(LibName)]
    public static partial void JPH_PhysicsSystem_GetConstraints(nint handle, nint* constraints, uint count);

    [LibraryImport(LibName)]
    public static partial void JPH_PhysicsSystem_DrawBodies(nint system, DrawSettings* settings, nint renderer);
    [LibraryImport(LibName)]
    public static partial void JPH_PhysicsSystem_DrawConstraints(nint system, nint renderer);
    [LibraryImport(LibName)]
    public static partial void JPH_PhysicsSystem_DrawConstraintLimits(nint system, nint renderer);
    [LibraryImport(LibName)]
    public static partial void JPH_PhysicsSystem_DrawConstraintReferenceFrame(nint system, nint renderer);

    /* Material */
    [LibraryImport(LibName)]
    public static partial nint JPH_PhysicsMaterial_Create();

    [LibraryImport(LibName)]
    public static partial void JPH_PhysicsMaterial_Destroy(nint handle);

    /* BodyInterface */
    [LibraryImport(LibName)]
    public static partial nint JPH_PhysicsSystem_GetBodyInterface(nint system);

    [LibraryImport(LibName)]
    public static partial nint JPH_PhysicsSystem_GetBodyInterfaceNoLock(nint system);

    [LibraryImport(LibName)]
    public static partial nint JPH_BodyInterface_CreateBody(nint handle, nint settings);

    [LibraryImport(LibName)]
    public static partial nint JPH_BodyInterface_CreateSoftBody(nint handle, nint settings);

    [LibraryImport(LibName)]
    public static partial uint JPH_BodyInterface_CreateAndAddBody(nint handle, nint bodyID, Activation activation);

    [LibraryImport(LibName)]
    public static partial nint JPH_BodyInterface_CreateBodyWithID(IntPtr handle, uint bodyID, IntPtr settings);

    [LibraryImport(LibName)]
    public static partial nint JPH_BodyInterface_CreateBodyWithoutID(IntPtr handle, IntPtr settings);

    [LibraryImport(LibName)]
    public static partial void JPH_BodyInterface_DestroyBody(IntPtr handle, uint bodyID);

    [LibraryImport(LibName)]
    public static partial void JPH_BodyInterface_DestroyBodyWithoutID(IntPtr handle, IntPtr body);

    [LibraryImport(LibName)]
    [return: MarshalAs(UnmanagedType.U1)]
    public static partial bool JPH_BodyInterface_AssignBodyID(IntPtr handle, IntPtr body);

    [LibraryImport(LibName)]
    [return: MarshalAs(UnmanagedType.U1)]
    public static partial bool JPH_BodyInterface_AssignBodyID2(IntPtr handle, IntPtr body, uint bodyID);

    [LibraryImport(LibName)]
    public static partial IntPtr JPH_BodyInterface_UnassignBodyID(IntPtr handle, uint bodyID);

    [LibraryImport(LibName)]
    public static partial void JPH_BodyInterface_AddBody(nint handle, uint bodyID, Activation activationMode);

    [LibraryImport(LibName)]
    public static partial void JPH_BodyInterface_RemoveBody(nint handle, uint bodyID);

    [LibraryImport(LibName)]
    public static partial void JPH_BodyInterface_RemoveAndDestroyBody(nint handle, uint bodyID);

    [LibraryImport(LibName)]
    public static partial void JPH_BodyInterface_GetLinearVelocity(nint handle, uint bodyID, Vector3* velocity);

    [LibraryImport(LibName)]
    public static partial void JPH_BodyInterface_SetLinearVelocity(nint handle, uint bodyID, Vector3* velocity);

    [LibraryImport(LibName, EntryPoint = nameof(JPH_BodyInterface_GetCenterOfMassPosition))]
    public static partial void JPH_BodyInterface_GetCenterOfMassPosition(nint handle, uint bodyID, Vector3* position);

    [LibraryImport(LibName, EntryPoint = nameof(JPH_BodyInterface_GetCenterOfMassPosition))]
    public static partial void JPH_BodyInterface_GetCenterOfMassPositionDouble(nint handle, uint bodyID, Double3* position);

    [LibraryImport(LibName)]

    [return: MarshalAs(UnmanagedType.U1)]
    public static partial bool JPH_BodyInterface_IsActive(IntPtr handle, uint bodyID);

    [LibraryImport(LibName)]

    [return: MarshalAs(UnmanagedType.U1)]
    public static partial bool JPH_BodyInterface_IsAdded(IntPtr handle, uint bodyID);

    [LibraryImport(LibName)]
    public static partial BodyType JPH_BodyInterface_GetBodyType(IntPtr handle, uint bodyID);

    [LibraryImport(LibName)]
    public static partial MotionType JPH_BodyInterface_GetMotionType(IntPtr handle, uint bodyID);

    [LibraryImport(LibName)]
    public static partial void JPH_BodyInterface_SetMotionType(IntPtr handle, uint bodyID, MotionType motionType, Activation activationMode);

    [LibraryImport(LibName)]
    public static partial MotionQuality JPH_BodyInterface_GetMotionQuality(IntPtr handle, uint bodyID);

    [LibraryImport(LibName)]
    public static partial void JPH_BodyInterface_SetMotionQuality(IntPtr handle, uint bodyID, MotionQuality quality);

    [LibraryImport(LibName)]
    public static partial float JPH_BodyInterface_GetRestitution(IntPtr handle, uint bodyID);

    [LibraryImport(LibName)]
    public static partial void JPH_BodyInterface_SetRestitution(IntPtr handle, uint bodyID, float value);

    [LibraryImport(LibName)]
    public static partial float JPH_BodyInterface_GetFriction(IntPtr handle, uint bodyID);

    [LibraryImport(LibName)]
    public static partial void JPH_BodyInterface_SetFriction(IntPtr handle, uint bodyID, float value);

    [LibraryImport(LibName, EntryPoint = nameof(JPH_BodyInterface_SetPosition))]
    public static partial void JPH_BodyInterface_SetPosition(IntPtr handle, uint bodyId, Vector3* position, Activation activationMode);

    [LibraryImport(LibName, EntryPoint = nameof(JPH_BodyInterface_SetPosition))]
    public static partial void JPH_BodyInterface_SetPositionDouble(IntPtr handle, uint bodyId, Double3* position, Activation activationMode);

    [LibraryImport(LibName, EntryPoint = nameof(JPH_BodyInterface_GetPosition))]
    public static partial void JPH_BodyInterface_GetPosition(IntPtr handle, uint bodyId, Vector3* position);

    [LibraryImport(LibName, EntryPoint = nameof(JPH_BodyInterface_GetPosition))]
    public static partial void JPH_BodyInterface_GetPositionDouble(IntPtr handle, uint bodyId, Double3* position);

    [LibraryImport(LibName)]
    public static partial void JPH_BodyInterface_SetRotation(IntPtr handle, uint bodyId, in Quaternion rotation, Activation activationMode);

    [LibraryImport(LibName)]
    public static partial void JPH_BodyInterface_GetRotation(IntPtr handle, uint bodyId, out Quaternion rotation);

    [LibraryImport(LibName, EntryPoint = nameof(JPH_BodyInterface_SetPositionAndRotation))]
    public static partial void JPH_BodyInterface_SetPositionAndRotation(IntPtr handle, uint bodyID, Vector3* position, Quaternion* rotation, Activation activationMode);

    [LibraryImport(LibName, EntryPoint = nameof(JPH_BodyInterface_SetPositionAndRotation))]
    public static partial void JPH_BodyInterface_SetPositionAndRotationDouble(IntPtr handle, uint bodyID, Double3* position, Quaternion* rotation, Activation activationMode);

    [LibraryImport(LibName, EntryPoint = nameof(JPH_BodyInterface_SetPositionAndRotationWhenChanged))]
    public static partial void JPH_BodyInterface_SetPositionAndRotationWhenChanged(IntPtr handle, uint bodyID, Vector3* position, Quaternion* rotation, Activation activationMode);

    [LibraryImport(LibName, EntryPoint = nameof(JPH_BodyInterface_SetPositionAndRotationWhenChanged))]
    public static partial void JPH_BodyInterface_SetPositionAndRotationWhenChangedDouble(IntPtr handle, uint bodyID, Double3* position, Quaternion* rotation, Activation activationMode);

    [LibraryImport(LibName, EntryPoint = nameof(JPH_BodyInterface_SetPositionRotationAndVelocity))]
    public static partial void JPH_BodyInterface_SetPositionRotationAndVelocity(IntPtr handle, uint bodyID, Vector3* position, Quaternion* rotation, Vector3* linearVelocity, Vector3* angularVelocity);

    [LibraryImport(LibName, EntryPoint = nameof(JPH_BodyInterface_SetPositionRotationAndVelocity))]
    public static partial void JPH_BodyInterface_SetPositionRotationAndVelocityDouble(nint handle, uint bodyID, Double3* position, Quaternion* rotation, Vector3* linearVelocity, Vector3* angularVelocity);

    [LibraryImport(LibName)]
    public static partial nint JPH_BodyInterface_GetShape(nint handle, uint bodyId);

    [LibraryImport(LibName)]
    public static partial void JPH_BodyInterface_SetShape(nint handle, uint bodyId, nint shape, [MarshalAs(UnmanagedType.U1)] bool updateMassProperties, Activation activationMode);

    [LibraryImport(LibName)]
    public static partial void JPH_BodyInterface_NotifyShapeChanged(nint handle, uint bodyId, Vector3* previousCenterOfMass, [MarshalAs(UnmanagedType.U1)] bool updateMassProperties, Activation activationMode);

    [LibraryImport(LibName)]
    public static partial void JPH_BodyInterface_ActivateBody(IntPtr handle, uint bodyId);

    [LibraryImport(LibName)]
    public static partial void JPH_BodyInterface_DeactivateBody(IntPtr handle, uint bodyId);

    [LibraryImport(LibName)]
    public static partial void JPH_BodyInterface_SetObjectLayer(IntPtr handle, uint bodyId, ushort layer);

    [LibraryImport(LibName)]
    public static partial ushort JPH_BodyInterface_GetObjectLayer(IntPtr handle, uint bodyId);

    [LibraryImport(LibName, EntryPoint = nameof(JPH_BodyInterface_GetWorldTransform))]
    public static partial void JPH_BodyInterface_GetWorldTransform(nint handle, uint bodyId, Matrix4x4* result);

    [LibraryImport(LibName, EntryPoint = nameof(JPH_BodyInterface_GetWorldTransform))]
    public static partial void JPH_BodyInterface_GetWorldTransformDouble(nint handle, uint bodyId, RMatrix4x4* result);

    [LibraryImport(LibName, EntryPoint = nameof(JPH_BodyInterface_GetCenterOfMassTransform))]
    public static partial void JPH_BodyInterface_GetCenterOfMassTransform(nint handle, uint bodyId, Matrix4x4* result);

    [LibraryImport(LibName, EntryPoint = nameof(JPH_BodyInterface_GetCenterOfMassTransform))]
    public static partial void JPH_BodyInterface_GetCenterOfMassTransformDouble(nint handle, uint bodyId, RMatrix4x4* result);

    [LibraryImport(LibName)]
    public static partial void JPH_BodyInterface_MoveKinematic(nint handle, uint bodyId, in Vector3 targetPosition, in Quaternion targetRotation, float deltaTime);

    [LibraryImport(LibName)]
    public static partial void JPH_BodyInterface_MoveKinematic(nint handle, uint bodyId, in Double3 targetPosition, in Quaternion targetRotation, float deltaTime);

    [LibraryImport(LibName)]
    [return: MarshalAs(UnmanagedType.U1)]
    public static partial bool JPH_BodyInterface_ApplyBuoyancyImpulse(nint handle, in BodyID bodyId, in Vector3 surfacePosition, in Vector3 surfaceNormal, float buoyancy, float linearDrag, float angularDrag, in Vector3 fluidVelocity, in Vector3 gravity, float deltaTime);

    [LibraryImport(LibName)]
    [return: MarshalAs(UnmanagedType.U1)]
    public static partial bool JPH_BodyInterface_ApplyBuoyancyImpulse(nint handle, in BodyID bodyId, in Double3 surfacePosition, in Vector3 surfaceNormal, float buoyancy, float linearDrag, float angularDrag, in Vector3 fluidVelocity, in Vector3 gravity, float deltaTime);

    [LibraryImport(LibName)]
    public static partial void JPH_BodyInterface_SetLinearAndAngularVelocity(nint handle, uint bodyId, in Vector3 linearVelocity, in Vector3 angularVelocity);

    [LibraryImport(LibName)]
    public static partial void JPH_BodyInterface_GetLinearAndAngularVelocity(nint handle, uint bodyId, out Vector3 linearVelocity, out Vector3 angularVelocity);

    [LibraryImport(LibName)]
    public static partial void JPH_BodyInterface_AddLinearVelocity(nint handle, uint bodyId, in Vector3 linearVelocity);

    [LibraryImport(LibName)]
    public static partial void JPH_BodyInterface_AddLinearAndAngularVelocity(nint handle, uint bodyId, in Vector3 linearVelocity, in Vector3 angularVelocity);

    [LibraryImport(LibName)]
    public static partial void JPH_BodyInterface_SetAngularVelocity(nint handle, uint bodyId, in Vector3 angularVelocity);

    [LibraryImport(LibName)]
    public static partial void JPH_BodyInterface_GetAngularVelocity(nint handle, uint bodyId, out Vector3 angularVelocity);

    [LibraryImport(LibName)]
    public static partial void JPH_BodyInterface_GetPointVelocity(nint handle, uint bodyId, in /*RVec3*/Vector3* point, Vector3* velocity);

    [LibraryImport(LibName)]
    public static partial void JPH_BodyInterface_AddForce(nint handle, uint bodyId, in Vector3 force);

    [LibraryImport(LibName)]
    public static partial void JPH_BodyInterface_AddForce2(nint handle, uint bodyId, in Vector3 force, in /*RVec3*/Vector3 point);

    [LibraryImport(LibName)]
    public static partial void JPH_BodyInterface_AddTorque(nint handle, uint bodyId, in Vector3 torque);

    [LibraryImport(LibName)]
    public static partial void JPH_BodyInterface_AddForceAndTorque(IntPtr handle, uint bodyId, in Vector3 force, in Vector3 torque);

    [LibraryImport(LibName)]
    public static partial void JPH_BodyInterface_AddImpulse(IntPtr handle, uint bodyId, in Vector3 impulse);

    [LibraryImport(LibName)]
    public static partial void JPH_BodyInterface_AddImpulse2(IntPtr handle, uint bodyId, in Vector3 impulse, in /*RVec3*/Vector3 point);

    [LibraryImport(LibName)]
    public static partial void JPH_BodyInterface_AddAngularImpulse(IntPtr handle, uint bodyId, in Vector3 angularImpulse);

    [LibraryImport(LibName)]
    public static partial void JPH_BodyInterface_GetInverseInertia(IntPtr handle, uint bodyId, out Matrix4x4 result);

    [LibraryImport(LibName)]
    public static partial void JPH_BodyInterface_SetGravityFactor(IntPtr handle, uint bodyId, float gravityFactor);

    [LibraryImport(LibName)]
    public static partial float JPH_BodyInterface_GetGravityFactor(IntPtr handle, uint bodyId);

    [LibraryImport(LibName)]
    public static partial void JPH_BodyInterface_InvalidateContactCache(IntPtr handle, uint bodyId);

    [LibraryImport(LibName)]
    public static partial void JPH_BodyInterface_SetUserData(IntPtr handle, uint bodyId, ulong userData);

    [LibraryImport(LibName)]
    public static partial ulong JPH_BodyInterface_GetUserData(IntPtr handle, uint bodyId);

    /* BodyLockInterface */
    [LibraryImport(LibName)]
    public static partial nint JPH_PhysicsSystem_GetBodyLockInterface(nint system);

    [LibraryImport(LibName)]
    public static partial nint JPH_PhysicsSystem_GetBodyLockInterfaceNoLock(nint system);

    [LibraryImport(LibName)]
    public static partial void JPH_BodyLockInterface_LockRead(nint lockInterface, uint bodyID, out BodyLockRead @lock);

    [LibraryImport(LibName)]
    public static partial void JPH_BodyLockInterface_UnlockRead(nint lockInterface, in BodyLockRead @lock);

    [LibraryImport(LibName)]
    public static partial void JPH_BodyLockInterface_LockWrite(nint lockInterface, uint bodyID, out BodyLockWrite @lock);

    [LibraryImport(LibName)]
    public static partial void JPH_BodyLockInterface_UnlockWrite(nint lockInterface, in BodyLockWrite @lock);

    /* JPH_MotionProperties */
    [LibraryImport(LibName)]
    public static partial AllowedDOFs JPH_MotionProperties_GetAllowedDOFs(nint properties);

    [LibraryImport(LibName)]
    public static partial void JPH_MotionProperties_SetLinearDamping(nint properties, float damping);

    [LibraryImport(LibName)]
    public static partial float JPH_MotionProperties_GetLinearDamping(nint properties);

    [LibraryImport(LibName)]
    public static partial void JPH_MotionProperties_SetAngularDamping(nint properties, float damping);

    [LibraryImport(LibName)]
    public static partial float JPH_MotionProperties_GetAngularDamping(nint properties);

    [LibraryImport(LibName)]
    public static partial float JPH_MotionProperties_GetInverseMassUnchecked(nint properties);

    [LibraryImport(LibName)]
    public static partial float JPH_MotionProperties_SetMassProperties(nint properties, AllowedDOFs allowedDOFs, MassProperties* massProperties);

    [LibraryImport(LibName)]
    public static partial void JPH_MotionProperties_SetInverseMass(nint properties, float inverseMass);
    [LibraryImport(LibName)]
    public static partial void JPH_MotionProperties_GetInverseInertiaDiagonal(nint properties, Vector3* result);
    [LibraryImport(LibName)]
    public static partial void JPH_MotionProperties_GetInertiaRotation(nint properties, Quaternion* result);
    [LibraryImport(LibName)]
    public static partial void JPH_MotionProperties_SetInverseInertia(nint properties, Vector3* diagonal, Quaternion* rot);

    [LibraryImport(LibName)]
    public static partial void JPH_MassProperties_DecomposePrincipalMomentsOfInertia(in MassProperties properties, out Matrix4x4 rotation, out Vector3 diagonal);

    [LibraryImport(LibName)]
    public static partial void JPH_MassProperties_ScaleToMass(MassProperties* properties, float mass);

    [LibraryImport(LibName)]
    public static partial void JPH_MassProperties_GetEquivalentSolidBoxSize(float mass, in Vector3 inertiaDiagonal, out Vector3 result);


    /* BodyLockInterface */
    [LibraryImport(LibName)]
    public static partial nint JPH_PhysicsSystem_GetBroadPhaseQuery(nint system);

    [LibraryImport(LibName)]
    public static partial nint JPH_PhysicsSystem_GetNarrowPhaseQuery(nint system);

    [LibraryImport(LibName)]
    public static partial nint JPH_PhysicsSystem_GetNarrowPhaseQueryNoLock(nint system);

    #region BroadPhaseQuery
    [LibraryImport(LibName)]
    [return: MarshalAs(UnmanagedType.U1)]
    public static partial bool JPH_BroadPhaseQuery_CastRay(nint query,
        Vector3* origin, Vector3* direction,
        delegate* unmanaged<void*, BroadPhaseCastResult*, float> callback, void* userData,
        nint broadPhaseLayerFilter,
        nint objectLayerFilter);

    [LibraryImport(LibName)]
    [return: MarshalAs(UnmanagedType.U1)]
    public static partial bool JPH_BroadPhaseQuery_CollideAABox(nint query,
        BoundingBox* box,
        delegate* unmanaged<void*, in BodyID, void>* callback, void* userData,
        nint broadPhaseLayerFilter,
        nint objectLayerFilter);

    [LibraryImport(LibName)]
    [return: MarshalAs(UnmanagedType.U1)]
    public static partial bool JPH_BroadPhaseQuery_CollideSphere(nint query,
        Vector3* center, float radius,
        delegate* unmanaged<void*, in BodyID, void> callback, void* userData,
        nint broadPhaseLayerFilter,
        nint objectLayerFilter);

    [LibraryImport(LibName)]
    [return: MarshalAs(UnmanagedType.U1)]
    public static partial bool JPH_BroadPhaseQuery_CollidePoint(nint query,
        Vector3* point,
        delegate* unmanaged<void*, in BodyID, void> callback, void* userData,
        nint broadPhaseLayerFilter,
        nint objectLayerFilter);
    #endregion

    #region NarrowPhaseQuery
    [LibraryImport(LibName, EntryPoint = nameof(JPH_NarrowPhaseQuery_CastRay))]
    [return: MarshalAs(UnmanagedType.U1)]
    public static partial bool JPH_NarrowPhaseQuery_CastRay(nint system,
        Vector3* origin, Vector3* direction,
        /*out*/ RayCastResult* hit,
        IntPtr broadPhaseLayerFilter,
        IntPtr objectLayerFilter,
        IntPtr bodyFilter);

    [LibraryImport(LibName, EntryPoint = nameof(JPH_NarrowPhaseQuery_CastRay))]
    [return: MarshalAs(UnmanagedType.U1)]
    public static partial bool JPH_NarrowPhaseQuery_CastRayDouble(nint system,
        Double3* origin, Vector3* direction,
        /*out*/ RayCastResult* hit,
        IntPtr broadPhaseLayerFilter,
        IntPtr objectLayerFilter,
        IntPtr bodyFilter);

    [LibraryImport(LibName, EntryPoint = nameof(JPH_NarrowPhaseQuery_CastRay2))]
    [return: MarshalAs(UnmanagedType.U1)]
    public static partial bool JPH_NarrowPhaseQuery_CastRay2(nint system,
        Vector3* origin, Vector3* direction,
        delegate* unmanaged<void*, RayCastResult*, float> callback, void* userData,
        IntPtr broadPhaseLayerFilter,
        IntPtr objectLayerFilter,
        IntPtr bodyFilter);

    [LibraryImport(LibName, EntryPoint = nameof(JPH_NarrowPhaseQuery_CastRay2))]
    [return: MarshalAs(UnmanagedType.U1)]
    public static partial bool JPH_NarrowPhaseQuery_CastRay2Double(nint system,
        Double3* origin, Vector3* direction,
        delegate* unmanaged<void*, RayCastResult*, float> callback, void* userData,
        nint broadPhaseLayerFilter,
        nint objectLayerFilter,
        nint bodyFilter);

    [LibraryImport(LibName, EntryPoint = nameof(JPH_NarrowPhaseQuery_CollidePoint))]
    [return: MarshalAs(UnmanagedType.U1)]
    public static partial bool JPH_NarrowPhaseQuery_CollidePoint(nint query,
        Vector3* point,
        delegate* unmanaged<void*, CollidePointResult*, float> callback, void* userData,
        nint broadPhaseLayerFilter,
        nint objectLayerFilter,
        nint bodyFilter
        );

    [LibraryImport(LibName, EntryPoint = nameof(JPH_NarrowPhaseQuery_CollidePoint))]
    [return: MarshalAs(UnmanagedType.U1)]
    public static partial bool JPH_NarrowPhaseQuery_CollidePointDouble(nint query,
        Double3* point,
        delegate* unmanaged<void*, CollidePointResult*, float> callback, void* userData,
        nint broadPhaseLayerFilter,
        nint objectLayerFilter,
        nint bodyFilter
        );

    [LibraryImport(LibName, EntryPoint = nameof(JPH_NarrowPhaseQuery_CollideShape))]
    [return: MarshalAs(UnmanagedType.U1)]
    public static partial bool JPH_NarrowPhaseQuery_CollideShape(nint query,
        nint shape, Vector3* scale, Matrix4x4* centerOfMassTransform, Vector3* baseOffset,
        delegate* unmanaged<void*, CollideShapeResult*, float> callback, void* userData,
        nint broadPhaseLayerFilter,
        nint objectLayerFilter,
        nint bodyFilter
        );

    [LibraryImport(LibName, EntryPoint = nameof(JPH_NarrowPhaseQuery_CollideShape))]
    [return: MarshalAs(UnmanagedType.U1)]
    public static partial bool JPH_NarrowPhaseQuery_CollideShapeDouble(nint query,
        nint shape, Vector3* scale, RMatrix4x4* centerOfMassTransform, Double3* baseOffset,
        delegate* unmanaged<void*, CollideShapeResult*, float> callback, void* userData,
        nint broadPhaseLayerFilter,
        nint objectLayerFilter,
        nint bodyFilter
        );

    [LibraryImport(LibName, EntryPoint = nameof(JPH_NarrowPhaseQuery_CastShape))]
    [return: MarshalAs(UnmanagedType.U1)]
    public static partial bool JPH_NarrowPhaseQuery_CastShape(nint query,
        nint shape,
        Matrix4x4* centerOfMassTransform, Vector3* direction, Vector3* baseOffset,
        delegate* unmanaged<void*, ShapeCastResult*, float> callback, void* userData,
        nint broadPhaseLayerFilter,
        nint objectLayerFilter,
        nint bodyFilter
        );

    [LibraryImport(LibName, EntryPoint = nameof(JPH_NarrowPhaseQuery_CastShape))]
    [return: MarshalAs(UnmanagedType.U1)]
    public static partial bool JPH_NarrowPhaseQuery_CastShapeDouble(nint query,
        nint shape,
        RMatrix4x4* centerOfMassTransform, Vector3* direction, Double3* baseOffset,
        delegate* unmanaged<void*, ShapeCastResult*, float> callback, void* userData,
        nint broadPhaseLayerFilter,
        nint objectLayerFilter,
        nint bodyFilter
        );
    #endregion

    /* Body */
    [LibraryImport(LibName)]
    public static partial uint JPH_Body_GetID(nint body);

    [LibraryImport(LibName)]
    public static partial BodyType JPH_Body_GetBodyType(nint body);

    [LibraryImport(LibName)]
    public static partial void JPH_Body_GetWorldSpaceBounds(nint body, BoundingBox* box);

    [LibraryImport(LibName, EntryPoint = nameof(JPH_Body_GetWorldSpaceSurfaceNormal))]
    public static partial void JPH_Body_GetWorldSpaceSurfaceNormal(nint body, SubShapeID subShapeID, Vector3* position, Vector3* normal);

    [LibraryImport(LibName, EntryPoint = nameof(JPH_Body_GetWorldSpaceSurfaceNormal))]
    public static partial void JPH_Body_GetWorldSpaceSurfaceNormalDouble(nint body, SubShapeID subShapeID, Double3* position, Vector3* normal);

    [LibraryImport(LibName)]
    [return: MarshalAs(UnmanagedType.U1)]
    public static partial bool JPH_Body_IsActive(nint handle);

    [LibraryImport(LibName)]
    [return: MarshalAs(UnmanagedType.U1)]
    public static partial bool JPH_Body_IsStatic(nint handle);

    [LibraryImport(LibName)]
    [return: MarshalAs(UnmanagedType.U1)]
    public static partial bool JPH_Body_IsKinematic(nint handle);

    [LibraryImport(LibName)]
    [return: MarshalAs(UnmanagedType.U1)]
    public static partial bool JPH_Body_IsDynamic(nint handle);

    [LibraryImport(LibName)]
    [return: MarshalAs(UnmanagedType.U1)]
    public static partial bool JPH_Body_IsSensor(nint handle);

    [LibraryImport(LibName)]
    public static partial void JPH_Body_SetIsSensor(nint handle, [MarshalAs(UnmanagedType.U1)] bool value);

    [LibraryImport(LibName)]
    public static partial void JPH_Body_SetCollideKinematicVsNonDynamic(nint handle, [MarshalAs(UnmanagedType.U1)] bool value);

    [LibraryImport(LibName)]
    [return: MarshalAs(UnmanagedType.U1)]
    public static partial bool JPH_Body_GetCollideKinematicVsNonDynamic(nint handle);

    [LibraryImport(LibName)]
    public static partial void JPH_Body_SetUseManifoldReduction(nint handle, [MarshalAs(UnmanagedType.U1)] bool value);

    [LibraryImport(LibName)]
    [return: MarshalAs(UnmanagedType.U1)]
    public static partial bool JPH_Body_GetUseManifoldReduction(nint handle);

    [LibraryImport(LibName)]
    [return: MarshalAs(UnmanagedType.U1)]
    public static partial bool JPH_Body_GetUseManifoldReductionWithBody(nint handle, nint other);

    [LibraryImport(LibName)]
    public static partial void JPH_Body_SetApplyGyroscopicForce(nint handle, [MarshalAs(UnmanagedType.U1)] bool value);

    [LibraryImport(LibName)]
    [return: MarshalAs(UnmanagedType.U1)]
    public static partial bool JPH_Body_GetApplyGyroscopicForce(nint handle);

    [LibraryImport(LibName)]
    public static partial nint JPH_Body_GetMotionProperties(nint handle);

    [LibraryImport(LibName)]
    public static partial MotionType JPH_Body_GetMotionType(nint handle);

    [LibraryImport(LibName)]
    public static partial void JPH_Body_SetMotionType(nint handle, MotionType motionType);

    [LibraryImport(LibName)]
    [return: MarshalAs(UnmanagedType.U1)]
    public static partial bool JPH_Body_GetAllowSleeping(nint handle);

    [LibraryImport(LibName)]
    public static partial void JPH_Body_SetAllowSleeping(nint handle, [MarshalAs(UnmanagedType.U1)] bool motionType);

    [LibraryImport(LibName)]
    public static partial void JPH_Body_ResetSleepTimer(nint handle);

    [LibraryImport(LibName)]
    public static partial float JPH_Body_GetFriction(nint handle);

    [LibraryImport(LibName)]
    public static partial void JPH_Body_SetFriction(nint handle, float value);

    [LibraryImport(LibName)]
    public static partial float JPH_Body_GetRestitution(nint handle);

    [LibraryImport(LibName)]
    public static partial void JPH_Body_SetRestitution(nint handle, float value);

    [LibraryImport(LibName, EntryPoint = nameof(JPH_Body_GetPosition))]
    public static partial void JPH_Body_GetPosition(nint handle, Vector3* result);

    [LibraryImport(LibName, EntryPoint = nameof(JPH_Body_GetPosition))]
    public static partial void JPH_Body_GetPositionDouble(nint handle, Double3* result);

    [LibraryImport(LibName)]
    public static partial void JPH_Body_GetRotation(IntPtr handle, Quaternion* result);

    [LibraryImport(LibName)]
    public static partial void JPH_Body_GetCenterOfMassPosition(IntPtr handle, Vector3* result);

    [LibraryImport(LibName, EntryPoint = nameof(JPH_Body_GetCenterOfMassPosition))]
    public static partial void JPH_Body_GetCenterOfMassPositionDouble(IntPtr handle, Double3* result);

    [LibraryImport(LibName, EntryPoint = nameof(JPH_Body_GetWorldTransform))]
    public static partial void JPH_Body_GetWorldTransform(IntPtr handle, Matrix4x4* result);

    [LibraryImport(LibName, EntryPoint = nameof(JPH_Body_GetCenterOfMassTransform))]
    public static partial void JPH_Body_GetCenterOfMassTransform(IntPtr handle, Matrix4x4* result);

    [LibraryImport(LibName, EntryPoint = nameof(JPH_Body_GetWorldTransform))]
    public static partial void JPH_Body_GetWorldTransformDouble(IntPtr handle, RMatrix4x4* result);

    [LibraryImport(LibName, EntryPoint = nameof(JPH_Body_GetCenterOfMassTransform))]
    public static partial void JPH_Body_GetCenterOfMassTransformDouble(IntPtr handle, RMatrix4x4* result);

    [LibraryImport(LibName)]
    public static partial void JPH_Body_GetLinearVelocity(IntPtr handle, Vector3* result);

    [LibraryImport(LibName)]
    public static partial void JPH_Body_SetLinearVelocity(IntPtr handle, Vector3* velocity);

    [LibraryImport(LibName)]
    public static partial void JPH_Body_GetAngularVelocity(IntPtr handle, Vector3* velocity);

    [LibraryImport(LibName)]
    public static partial void JPH_Body_SetAngularVelocity(IntPtr handle, Vector3* velocity);

    [LibraryImport(LibName)]
    public static partial void JPH_Body_AddForce(IntPtr handle, Vector3* velocity);

    [LibraryImport(LibName)]
    public static partial void JPH_Body_AddForceAtPosition(IntPtr handle, Vector3* velocity, Vector3* position);

    [LibraryImport(LibName, EntryPoint = nameof(JPH_Body_AddForceAtPosition))]
    public static partial void JPH_Body_AddForceAtPositionDouble(IntPtr handle, Vector3* velocity, Double3* position);

    [LibraryImport(LibName)]
    public static partial void JPH_Body_AddTorque(IntPtr handle, Vector3* value);

    [LibraryImport(LibName)]
    public static partial void JPH_Body_GetAccumulatedForce(IntPtr handle, Vector3* force);

    [LibraryImport(LibName)]
    public static partial void JPH_Body_GetAccumulatedTorque(IntPtr handle, Vector3* force);

    [LibraryImport(LibName)]
    public static partial void JPH_Body_AddImpulse(nint handle, in Vector3 impulse);

    [LibraryImport(LibName)]
    public static partial void JPH_Body_AddImpulseAtPosition(nint handle, Vector3* impulse, Vector3* position);

    [LibraryImport(LibName, EntryPoint = nameof(JPH_Body_AddImpulseAtPosition))]
    public static partial void JPH_Body_AddImpulseAtPositionDouble(nint handle, Vector3* impulse, Double3* position);

    [LibraryImport(LibName)]
    public static partial void JPH_Body_AddAngularImpulse(nint handle, in Vector3 angularImpulse);

    [LibraryImport(LibName)]
    public static partial void JPH_Body_MoveKinematic(nint handle, in Vector3 targetPosition, in Quaternion targetRotation, float deltaTime);

    [LibraryImport(LibName)]
    public static partial void JPH_Body_MoveKinematic(nint handle, in Double3 targetPosition, in Quaternion targetRotation, float deltaTime);

    [LibraryImport(LibName)]
    [return: MarshalAs(UnmanagedType.U1)]
    public static partial bool JPH_Body_ApplyBuoyancyImpulse(nint handle, in Vector3 surfacePosition, in Vector3 surfaceNormal, float buoyancy, float linearDrag, float angularDrag, in Vector3 fluidVelocity, in Vector3 gravity, float deltaTime);

    [LibraryImport(LibName)]
    [return: MarshalAs(UnmanagedType.U1)]
    public static partial bool JPH_Body_ApplyBuoyancyImpulse(nint handle, in Double3 surfacePosition, in Vector3 surfaceNormal, float buoyancy, float linearDrag, float angularDrag, in Vector3 fluidVelocity, in Vector3 gravity, float deltaTime);


    [LibraryImport(LibName)]
    public static partial void JPH_Body_SetUserData(nint handle, ulong userData);

    [LibraryImport(LibName)]
    public static partial ulong JPH_Body_GetUserData(nint handle);

    // ContactListener
    public struct JPH_ContactListener_Procs
    {
        public delegate* unmanaged<nint, nint, nint, Vector3*, nint, uint> OnContactValidate;
        public delegate* unmanaged<nint, nint, nint, nint, nint, void> OnContactAdded;
        public delegate* unmanaged<nint, nint, nint, nint, nint, void> OnContactPersisted;
        public delegate* unmanaged<nint, SubShapeIDPair*, void> OnContactRemoved;
    }

    public struct JPH_ContactListener_ProcsDouble
    {
        public delegate* unmanaged<nint, nint, nint, Double3*, nint, uint> OnContactValidate;
        public delegate* unmanaged<nint, nint, nint, nint, nint, void> OnContactAdded;
        public delegate* unmanaged<nint, nint, nint, nint, nint, void> OnContactPersisted;
        public delegate* unmanaged<nint, SubShapeIDPair*, void> OnContactRemoved;
    }

    [LibraryImport(LibName, EntryPoint = nameof(JPH_ContactListener_SetProcs))]
    public static partial void JPH_ContactListener_SetProcs(nint listener, JPH_ContactListener_Procs procs, nint userData);

    [LibraryImport(LibName, EntryPoint = nameof(JPH_ContactListener_SetProcs))]
    public static partial void JPH_ContactListener_SetProcsDouble(nint listener, JPH_ContactListener_ProcsDouble procs, nint userData);

    [LibraryImport(LibName)]
    public static partial IntPtr JPH_ContactListener_Create();

    [LibraryImport(LibName)]
    public static partial void JPH_ContactListener_Destroy(IntPtr handle);

    // BodyActivationListener
    public struct JPH_BodyActivationListener_Procs
    {
        public delegate* unmanaged<nint, uint, ulong, void> OnBodyActivated;
        public delegate* unmanaged<nint, uint, ulong, void> OnBodyDeactivated;
    }

    [LibraryImport(LibName)]
    public static partial void JPH_BodyActivationListener_SetProcs(nint listener, JPH_BodyActivationListener_Procs procs, nint userData);

    [LibraryImport(LibName)]
    public static partial nint JPH_BodyActivationListener_Create();

    [LibraryImport(LibName)]
    public static partial void JPH_BodyActivationListener_Destroy(nint handle);

    /* ContactManifold */
    [LibraryImport(LibName)]
    public static partial void JPH_ContactManifold_GetWorldSpaceNormal(nint manifold, out Vector3 result);

    [LibraryImport(LibName)]
    public static partial float JPH_ContactManifold_GetPenetrationDepth(nint manifold);
    [LibraryImport(LibName)]
    public static partial SubShapeID JPH_ContactManifold_GetSubShapeID1(nint manifold);
    [LibraryImport(LibName)]
    public static partial SubShapeID JPH_ContactManifold_GetSubShapeID2(nint manifold);
    [LibraryImport(LibName)]
    public static partial uint JPH_ContactManifold_GetPointCount(nint manifold);
    [LibraryImport(LibName)]
    public static partial void JPH_ContactManifold_GetWorldSpaceContactPointOn1(nint manifold, uint index, Vector3* result); // JPH_RVec3
    [LibraryImport(LibName)]
    public static partial void JPH_ContactManifold_GetWorldSpaceContactPointOn2(nint manifold, uint index, Vector3* result); // JPH_RVec3

    /* ContactSettings */
    [LibraryImport(LibName)]
    public static partial float JPH_ContactSettings_GetFriction(nint settings);
    [LibraryImport(LibName)]
    public static partial void JPH_ContactSettings_SetFriction(nint settings, float friction);
    [LibraryImport(LibName)]
    public static partial float JPH_ContactSettings_GetRestitution(nint settings);
    [LibraryImport(LibName)]
    public static partial void JPH_ContactSettings_SetRestitution(nint settings, float restitution);
    [LibraryImport(LibName)]
    public static partial float JPH_ContactSettings_GetInvMassScale1(nint settings);
    [LibraryImport(LibName)]
    public static partial void JPH_ContactSettings_SetInvMassScale1(nint settings, float scale);
    [LibraryImport(LibName)]
    public static partial float JPH_ContactSettings_GetInvInertiaScale1(nint settings);
    [LibraryImport(LibName)]
    public static partial void JPH_ContactSettings_SetInvInertiaScale1(nint settings, float scale);
    [LibraryImport(LibName)]
    public static partial float JPH_ContactSettings_GetInvMassScale2(nint settings);
    [LibraryImport(LibName)]
    public static partial void JPH_ContactSettings_SetInvMassScale2(nint settings, float scale);
    [LibraryImport(LibName)]
    public static partial float JPH_ContactSettings_GetInvInertiaScale2(nint settings);
    [LibraryImport(LibName)]
    public static partial void JPH_ContactSettings_SetInvInertiaScale2(nint settings, float scale);
    [LibraryImport(LibName)]
    [return: MarshalAs(UnmanagedType.U1)]
    public static partial bool JPH_ContactSettings_GetIsSensor(nint settings);
    [LibraryImport(LibName)]
    public static partial void JPH_ContactSettings_SetIsSensor(nint settings, [MarshalAs(UnmanagedType.U1)] bool sensor);
    [LibraryImport(LibName)]
    public static partial void JPH_ContactSettings_GetRelativeLinearSurfaceVelocity(nint settings, Vector3* result);
    [LibraryImport(LibName)]
    public static partial void JPH_ContactSettings_SetRelativeLinearSurfaceVelocity(nint settings, Vector3* velocity);
    [LibraryImport(LibName)]
    public static partial void JPH_ContactSettings_GetRelativeAngularSurfaceVelocity(nint settings, Vector3* result);
    [LibraryImport(LibName)]
    public static partial void JPH_ContactSettings_SetRelativeAngularSurfaceVelocity(nint settings, Vector3* velocity);

    #region CharacterBase
    /* CharacterBaseSettings */
    [LibraryImport(LibName)]
    public static partial void JPH_CharacterBaseSettings_Destroy(nint handle);

    [LibraryImport(LibName)]
    public static partial void JPH_CharacterBaseSettings_GetUp(nint handle, out Vector3 result);

    [LibraryImport(LibName)]
    public static partial void JPH_CharacterBaseSettings_SetUp(nint handle, in Vector3 value);

    [LibraryImport(LibName)]
    public static partial void JPH_CharacterBaseSettings_GetSupportingVolume(nint handle, out Plane result);

    [LibraryImport(LibName)]
    public static partial void JPH_CharacterBaseSettings_SetSupportingVolume(nint handle, in Plane value);

    [LibraryImport(LibName)]
    public static partial float JPH_CharacterBaseSettings_GetMaxSlopeAngle(nint handle);

    [LibraryImport(LibName)]
    public static partial void JPH_CharacterBaseSettings_SetMaxSlopeAngle(nint handle, float value);

    [LibraryImport(LibName)]
    [return: MarshalAs(UnmanagedType.U1)]
    public static partial bool JPH_CharacterBaseSettings_GetEnhancedInternalEdgeRemoval(nint handle);

    [LibraryImport(LibName)]
    public static partial void JPH_CharacterBaseSettings_SetEnhancedInternalEdgeRemoval(nint handle, [MarshalAs(UnmanagedType.U1)] bool value);

    [LibraryImport(LibName)]
    public static partial nint JPH_CharacterBaseSettings_GetShape(nint handle);

    [LibraryImport(LibName)]
    public static partial void JPH_CharacterBaseSettings_SetShape(nint handle, nint shape);

    /* CharacterBase */
    [LibraryImport(LibName)]
    public static partial void JPH_CharacterBase_Destroy(nint handle);

    [LibraryImport(LibName)]
    public static partial float JPH_CharacterBase_GetCosMaxSlopeAngle(nint handle);

    [LibraryImport(LibName)]
    public static partial void JPH_CharacterBase_SetMaxSlopeAngle(nint handle, float value);

    [LibraryImport(LibName)]
    public static partial void JPH_CharacterBase_GetUp(nint handle, out Vector3 result);

    [LibraryImport(LibName)]
    public static partial void JPH_CharacterBase_SetUp(nint handle, in Vector3 value);

    [LibraryImport(LibName)]
    [return: MarshalAs(UnmanagedType.U1)]
    public static partial bool JPH_CharacterBase_IsSlopeTooSteep(nint handle, in Vector3 value);

    [LibraryImport(LibName)]
    public static partial nint JPH_CharacterBase_GetShape(nint handle);

    [LibraryImport(LibName)]
    public static partial GroundState JPH_CharacterBase_GetGroundState(nint handle);

    [LibraryImport(LibName)]
    [return: MarshalAs(UnmanagedType.U1)]
    public static partial bool JPH_CharacterBase_IsSupported(nint handle);

    [LibraryImport(LibName)]
    public static partial void JPH_CharacterBase_GetGroundPosition(nint handle, out Vector3 position); // RVec3

    [LibraryImport(LibName)]
    public static partial void JPH_CharacterBase_GetGroundNormal(nint handle, out Vector3 normal);

    [LibraryImport(LibName)]
    public static partial void JPH_CharacterBase_GetGroundVelocity(nint handle, out Vector3 velocity);

    [LibraryImport(LibName)]
    public static partial nint JPH_CharacterBase_GetGroundMaterial(nint handle);

    [LibraryImport(LibName)]
    public static partial uint JPH_CharacterBase_GetGroundBodyId(nint handle);

    [LibraryImport(LibName)]
    public static partial uint JPH_CharacterBase_GetGroundSubShapeId(nint handle);

    [LibraryImport(LibName)]
    public static partial ulong JPH_CharacterBase_GetGroundUserData(nint handle);
    #endregion

    #region Characted
    /* CharacterSettings */
    [LibraryImport(LibName)]
    public static partial nint JPH_CharacterSettings_Create();
    [LibraryImport(LibName)]
    public static partial ObjectLayer JPH_CharacterSettings_GetLayer(nint settings);
    [LibraryImport(LibName)]
    public static partial void JPH_CharacterSettings_SetLayer(nint settings, ObjectLayer value);
    [LibraryImport(LibName)]
    public static partial float JPH_CharacterSettings_GetMass(nint settings);
    [LibraryImport(LibName)]
    public static partial void JPH_CharacterSettings_SetMass(nint settings, float value);
    [LibraryImport(LibName)]
    public static partial float JPH_CharacterSettings_GetFriction(nint settings);
    [LibraryImport(LibName)]
    public static partial void JPH_CharacterSettings_SetFriction(nint settings, float value);
    [LibraryImport(LibName)]
    public static partial float JPH_CharacterSettings_GetGravityFactor(nint settings);
    [LibraryImport(LibName)]
    public static partial void JPH_CharacterSettings_SetGravityFactor(nint settings, float value);

    /* Character */
    [LibraryImport(LibName)]
    public static partial nint JPH_Character_Create(nint settings, /*const JPH_RVec3**/ in Vector3 position, in Quaternion rotation, ulong userData,/*JPH_PhysicsSystem**/ nint system);

    [LibraryImport(LibName)]
    public static partial nint JPH_Character_Create(nint settings, in Double3 position, in Quaternion rotation, ulong userData, nint physicsSystem);

    [LibraryImport(LibName)]
    public static partial void JPH_Character_AddToPhysicsSystem(nint character, Activation activationMode /*= JPH_ActivationActivate */, [MarshalAs(UnmanagedType.U1)] bool lockBodies /* = true */);
    [LibraryImport(LibName)]
    public static partial void JPH_Character_RemoveFromPhysicsSystem(nint character, [MarshalAs(UnmanagedType.U1)] bool lockBodies /* = true */);
    [LibraryImport(LibName)]
    public static partial void JPH_Character_Activate(nint character, [MarshalAs(UnmanagedType.U1)] bool lockBodies /* = true */);
    [LibraryImport(LibName)]
    public static partial void JPH_Character_PostSimulation(nint character, float maxSeparationDistance, [MarshalAs(UnmanagedType.U1)] bool lockBodies /* = true */);
    #endregion

    #region CharacterVirtual
    /* CharacterVirtualSettings */
    [LibraryImport(LibName)]
    public static partial nint JPH_CharacterVirtualSettings_Create();

    [LibraryImport(LibName)]
    public static partial float JPH_CharacterVirtualSettings_GetMass(nint handle);
    [LibraryImport(LibName)]
    public static partial void JPH_CharacterVirtualSettings_SetMass(nint handle, float value);
    [LibraryImport(LibName)]
    public static partial float JPH_CharacterVirtualSettings_GetMaxStrength(nint settings);
    [LibraryImport(LibName)]
    public static partial void JPH_CharacterVirtualSettings_SetMaxStrength(nint settings, float value);
    [LibraryImport(LibName)]
    public static partial void JPH_CharacterVirtualSettings_GetShapeOffset(nint settings, out Vector3 result);
    [LibraryImport(LibName)]
    public static partial void JPH_CharacterVirtualSettings_SetShapeOffset(nint settings, in Vector3 value);
    [LibraryImport(LibName)]
    public static partial BackFaceMode JPH_CharacterVirtualSettings_GetBackFaceMode(nint settings);
    [LibraryImport(LibName)]
    public static partial void JPH_CharacterVirtualSettings_SetBackFaceMode(nint settings, BackFaceMode value);
    [LibraryImport(LibName)]
    public static partial float JPH_CharacterVirtualSettings_GetPredictiveContactDistance(nint settings);
    [LibraryImport(LibName)]
    public static partial void JPH_CharacterVirtualSettings_SetPredictiveContactDistance(nint settings, float value);
    [LibraryImport(LibName)]
    public static partial uint JPH_CharacterVirtualSettings_GetMaxCollisionIterations(nint settings);
    [LibraryImport(LibName)]
    public static partial void JPH_CharacterVirtualSettings_SetMaxCollisionIterations(nint settings, uint value);
    [LibraryImport(LibName)]
    public static partial uint JPH_CharacterVirtualSettings_GetMaxConstraintIterations(nint settings);
    [LibraryImport(LibName)]
    public static partial void JPH_CharacterVirtualSettings_SetMaxConstraintIterations(nint settings, uint value);
    [LibraryImport(LibName)]
    public static partial float JPH_CharacterVirtualSettings_GetMinTimeRemaining(nint settings);
    [LibraryImport(LibName)]
    public static partial void JPH_CharacterVirtualSettings_SetMinTimeRemaining(nint settings, float value);
    [LibraryImport(LibName)]
    public static partial float JPH_CharacterVirtualSettings_GetCollisionTolerance(nint settings);
    [LibraryImport(LibName)]
    public static partial void JPH_CharacterVirtualSettings_SetCollisionTolerance(nint settings, float value);
    [LibraryImport(LibName)]
    public static partial float JPH_CharacterVirtualSettings_GetCharacterPadding(nint settings);
    [LibraryImport(LibName)]
    public static partial void JPH_CharacterVirtualSettings_SetCharacterPadding(nint settings, float value);
    [LibraryImport(LibName)]
    public static partial uint JPH_CharacterVirtualSettings_GetMaxNumHits(nint settings);
    [LibraryImport(LibName)]
    public static partial void JPH_CharacterVirtualSettings_SetMaxNumHits(nint settings, uint value);
    [LibraryImport(LibName)]
    public static partial float JPH_CharacterVirtualSettings_GetHitReductionCosMaxAngle(nint settings);
    [LibraryImport(LibName)]
    public static partial void JPH_CharacterVirtualSettings_SetHitReductionCosMaxAngle(nint settings, float value);
    [LibraryImport(LibName)]
    public static partial float JPH_CharacterVirtualSettings_GetPenetrationRecoverySpeed(nint settings);
    [LibraryImport(LibName)]
    public static partial void JPH_CharacterVirtualSettings_SetPenetrationRecoverySpeed(nint settings, float value);

    /* CharacterVirtual */
    [LibraryImport(LibName)]
    public static partial nint JPH_CharacterVirtual_Create(nint settings, in Vector3 position, in Quaternion rotation, ulong userData, nint physicsSystem);

    [LibraryImport(LibName)]
    public static partial nint JPH_CharacterVirtual_Create(nint settings, in Double3 position, in Quaternion rotation, ulong userData, nint physicsSystem);

    [LibraryImport(LibName)]
    public static partial void JPH_CharacterVirtual_GetLinearVelocity(IntPtr handle, out Vector3 velocity);

    [LibraryImport(LibName)]
    public static partial void JPH_CharacterVirtual_SetLinearVelocity(IntPtr handle, in Vector3 velocity);

    [LibraryImport(LibName)]
    public static partial void JPH_CharacterVirtual_GetPosition(IntPtr handle, out Vector3 position); // RVec3

    [LibraryImport(LibName)]
    public static partial void JPH_CharacterVirtual_SetPosition(nint handle, in Vector3 position);// RVec3

    [LibraryImport(LibName)]
    public static partial void JPH_CharacterVirtual_GetRotation(nint handle, out Quaternion rotation);

    [LibraryImport(LibName)]
    public static partial void JPH_CharacterVirtual_SetRotation(nint handle, in Quaternion rotation);

    [LibraryImport(LibName)]
    public static partial void JPH_CharacterVirtual_GetWorldTransform(nint shape, out Matrix4x4 result); //RMatrix4x4

    [LibraryImport(LibName)]
    public static partial void JPH_CharacterVirtual_GetCenterOfMassTransform(nint shape, out Matrix4x4 result); //RMatrix4x4

    [LibraryImport(LibName)]
    public static partial float JPH_CharacterVirtual_GetMass(nint handle);
    [LibraryImport(LibName)]
    public static partial void JPH_CharacterVirtual_SetMass(nint handle, float value);
    [LibraryImport(LibName)]
    public static partial float JPH_CharacterVirtual_GetMaxStrength(nint settings);
    [LibraryImport(LibName)]
    public static partial void JPH_CharacterVirtual_SetMaxStrength(nint settings, float value);

    [LibraryImport(LibName)]
    public static partial float JPH_CharacterVirtual_GetPenetrationRecoverySpeed(nint character);
    [LibraryImport(LibName)]
    public static partial void JPH_CharacterVirtual_SetPenetrationRecoverySpeed(nint character, float value);
    [LibraryImport(LibName)]
    [return: MarshalAs(UnmanagedType.U1)]
    public static partial bool JPH_CharacterVirtual_GetEnhancedInternalEdgeRemoval(nint character);
    [LibraryImport(LibName)]
    public static partial void JPH_CharacterVirtual_SetEnhancedInternalEdgeRemoval(nint character, [MarshalAs(UnmanagedType.U1)] bool value);
    [LibraryImport(LibName)]
    public static partial float JPH_CharacterVirtual_GetCharacterPadding(nint character);
    [LibraryImport(LibName)]
    public static partial uint JPH_CharacterVirtual_GetMaxNumHits(nint character);
    [LibraryImport(LibName)]
    public static partial void JPH_CharacterVirtual_SetMaxNumHits(nint character, uint value);
    [LibraryImport(LibName)]
    public static partial float JPH_CharacterVirtual_GetHitReductionCosMaxAngle(nint character);
    [LibraryImport(LibName)]
    public static partial void JPH_CharacterVirtual_SetHitReductionCosMaxAngle(nint character, float value);
    [LibraryImport(LibName)]
    [return: MarshalAs(UnmanagedType.U1)]
    public static partial bool JPH_CharacterVirtual_GetMaxHitsExceeded(nint character);
    [LibraryImport(LibName)]
    public static partial ulong JPH_CharacterVirtual_GetUserData(nint character);
    [LibraryImport(LibName)]
    public static partial void JPH_CharacterVirtual_SetUserData(nint character, ulong value);

    [LibraryImport(LibName)]
    public static partial void JPH_CharacterVirtual_Update(nint handle, float deltaTime, ushort layer, nint physicsSytem);

    [LibraryImport(LibName)]
    public static partial void JPH_CharacterVirtual_ExtendedUpdate(nint handle, float deltaTime, ExtendedUpdateSettings* settings, ushort layer, nint physicsSytem);

    [LibraryImport(LibName)]
    public static partial void JPH_CharacterVirtual_RefreshContacts(nint handle, ushort layer, nint physicsSytem);

    [LibraryImport(LibName)]
    public static partial void JPH_CharacterVirtual_CancelVelocityTowardsSteepSlopes(nint handle, in Vector3 desiredVelocity, out Vector3 velocity);

    public struct JPH_CharacterContactListener_Procs
    {
        public delegate* unmanaged<nint, nint, nint, Vector3*, Vector3*, void> OnAdjustBodyVelocity;
        public delegate* unmanaged<nint, nint, BodyID, SubShapeID, Bool8> OnContactValidate;
        public delegate* unmanaged<nint, nint, BodyID, SubShapeID, Vector3*, Vector3*, CharacterContactSettings*, void> OnContactAdded;
        public delegate* unmanaged<nint, nint, BodyID, SubShapeID, Vector3*, Vector3*, Vector3*, nint, Vector3*, Vector3*, void> OnContactSolve;
    }

    [LibraryImport(LibName)]
    public static partial nint JPH_CharacterContactListener_Create(JPH_CharacterContactListener_Procs procs, nint userData);

    [LibraryImport(LibName)]
    public static partial void JPH_CharacterContactListener_Destroy(nint listener);
    #endregion

    #region DebugRenderer
    public struct JPH_DebugRenderer_Procs
    {
        public delegate* unmanaged<nint, Vector3*, Vector3*, uint, void> DrawLine;
        public delegate* unmanaged<nint, Vector3*, Vector3*, Vector3*, uint, DebugRenderer.CastShadow, void> DrawTriangle;
        public delegate* unmanaged<nint, Vector3*, byte*, uint, float, void> DrawText3D;
    }

    [LibraryImport(LibName)]
    public static partial nint JPH_DebugRenderer_Create(JPH_DebugRenderer_Procs procs, nint userData);

    [LibraryImport(LibName)]
    public static partial void JPH_DebugRenderer_Destroy(nint renderer);

    [LibraryImport(LibName)]
    public static partial void JPH_DebugRenderer_NextFrame(nint renderer);
    #endregion

    sealed class UTF8EncodingRelaxed : UTF8Encoding
    {
        public static new readonly UTF8EncodingRelaxed Default = new UTF8EncodingRelaxed();

        private UTF8EncodingRelaxed() : base(false, false)
        {
        }
    }
}
