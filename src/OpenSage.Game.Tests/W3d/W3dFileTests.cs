﻿using System.IO;
using OpenSage.Data.Big;
using OpenSage.Data.W3d;
using Xunit;
using Xunit.Abstractions;

namespace OpenSage.Data.Tests.W3d
{
    public class W3dFileTests
    {
        private readonly ITestOutputHelper _output;

        public W3dFileTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void CanReadW3dFiles()
        {
            InstalledFilesTestData.ReadFiles(".w3d", _output, entry =>
            {
                if (Path.GetFileName(entry.FilePath) == "UISabotr_idel.w3d" ||
                    Path.GetFileName(entry.FilePath) == "UISabotr_Jump.w3d" ||
                    Path.GetFileName(entry.FilePath) == "UISabotr_Left.w3d" ||
                    Path.GetFileName(entry.FilePath) == "UISabotr_Right.w3d" ||
                    Path.GetFileName(entry.FilePath) == "UISabotr_Up.w3d")
                {
                    return; // Animation files, unused? and seem to be corrupt.
                }

                var w3dFile = W3dFile.FromFileSystemEntry(entry);

                foreach (var mesh in w3dFile.Meshes)
                {
                    Assert.Equal((int) mesh.Header.NumVertices, mesh.Vertices.Length);

                    Assert.Equal((int) mesh.Header.NumTris, mesh.Triangles.Length);

                    Assert.Equal(mesh.Vertices.Length, mesh.Influences.Length);

                    Assert.Equal((int) mesh.MaterialInfo.PassCount, mesh.MaterialPasses.Length);

                    Assert.Equal((int) mesh.MaterialInfo.ShaderCount, mesh.Shaders.Length);

                    Assert.Equal(mesh.Vertices.Length, mesh.ShadeIndices.Length);

                    Assert.True(mesh.Materials.Length <= 16);

                    foreach (var material in mesh.Materials)
                    {
                        Assert.Equal(W3dVertexMaterialFlags.None, material.VertexMaterialInfo.Attributes);

                        var stage0Mapping = material.VertexMaterialInfo.Stage0Mapping;
                        Assert.True(stage0Mapping == W3dVertexMappingType.Uv
                            || stage0Mapping == W3dVertexMappingType.Environment
                            || stage0Mapping == W3dVertexMappingType.LinearOffset
                            || stage0Mapping == W3dVertexMappingType.Grid);

                        var stage1Mapping = material.VertexMaterialInfo.Stage1Mapping;
                        Assert.True(stage1Mapping == W3dVertexMappingType.Uv
                            || stage1Mapping == W3dVertexMappingType.LinearOffset);

                        Assert.Equal(0, material.VertexMaterialInfo.Translucency);
                    }

                    Assert.True(mesh.MaterialPasses.Length <= 2);

                    foreach (var materialPass in mesh.MaterialPasses)
                    {
                        Assert.True(materialPass.Dcg == null || materialPass.Dcg.Length == mesh.Vertices.Length);
                        Assert.Null(materialPass.Dig);
                        Assert.Null(materialPass.Scg);

                        Assert.True(materialPass.TextureStages.Count <= 2);

                        foreach (var textureStage in materialPass.TextureStages)
                        {
                            Assert.True(textureStage.TexCoords == null || textureStage.TexCoords.Length == mesh.Header.NumVertices);

                            Assert.Null(textureStage.PerFaceTexCoordIds);

                            var numTextureIds = textureStage.TextureIds.Length;
                            Assert.True(numTextureIds == 1 || numTextureIds == mesh.Header.NumTris);
                        }

                        var numShaderIds = materialPass.ShaderIds.Length;
                        Assert.True(numShaderIds == 1 || numShaderIds == mesh.Header.NumTris);

                        var numVertexMaterialIds = materialPass.VertexMaterialIds.Length;
                        Assert.True(numVertexMaterialIds == 1 || numVertexMaterialIds == mesh.Header.NumVertices);
                    }

                    Assert.True(mesh.Textures.Length <= 29);

                    foreach (var texture in mesh.Textures)
                    {
                        if (texture.TextureInfo != null)
                        {
                            Assert.Equal(1u, texture.TextureInfo.FrameCount);
                        }
                    }
                }
            });
        }

        [Fact]
        public void LoadW3dFromBigFile()
        {
            const string bigFilePath = @"C:\Program Files (x86)\Origin Games\Command and Conquer Generals Zero Hour\Command and Conquer Generals Zero Hour\W3DZH.big";
        
            using (var bigStream = File.OpenRead(bigFilePath))
            using (var bigArchive = new BigArchive(bigStream))
            {
                var entry = bigArchive.GetEntry(@"Art\W3D\ABBarracks_AC.W3D");

                var w3dFile = W3dFile.FromFileSystemEntry(new FileSystemEntry(null, entry.FullName, entry.Length, entry.Open));
                Assert.Equal(3, w3dFile.Meshes.Count);
            }
        }
    }
}