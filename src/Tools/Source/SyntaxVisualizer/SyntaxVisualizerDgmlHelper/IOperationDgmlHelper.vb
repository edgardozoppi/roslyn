' Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

Imports System.Runtime.CompilerServices
Imports System.Xml.Linq
Imports Microsoft.CodeAnalysis
Imports Roslyn.SyntaxVisualizer.Control
Imports <xmlns="http://schemas.microsoft.com/vs/2009/dgml">

Public Class IOperationDgmlOptions
    Public Property ShowSpan As Boolean = True
    Public Property ShowErrors As Boolean = True
    Public Property ShowText As Boolean = False
    Public Property ShowGroups As Boolean = False
End Class

Public Module IOperationDgmlHelper
    Private ReadOnly s_defaultOptions As New IOperationDgmlOptions
    Private Const s_MAX_LABEL_LENGTH = 30

    'Helpers that return the DGML representation of a SyntaxNode / SyntaxToken / SyntaxTrivia.
    'DGML is an XML-based format for directed graphs that can be rendered by Visual Studio.

#Region "ToDgml"
    <Extension()>
    Public Function ToIOperationDgml(node As SyntaxNode,
                                     semanticModel As SemanticModel,
                                     Optional options As IOperationDgmlOptions = Nothing) As XElement
        If options Is Nothing Then
            options = s_defaultOptions
        End If

        Dim dgml = GetDgmlTemplate(options)
        ProcessNodeOrIOperation(options, node, dgml, semanticModel)
        Return dgml
    End Function

    <Extension()>
    Public Function ToIOperationDgml(operation As IOperation,
                                     Optional options As IOperationDgmlOptions = Nothing) As XElement
        If options Is Nothing Then
            options = s_defaultOptions
        End If
        Dim dgml = GetDgmlTemplate(options)
        ProcessIOperation(options, operation, dgml)
        Return dgml
    End Function
#End Region

#Region "Process*"
    Private Sub ProcessNodeOrIOperation(options As IOperationDgmlOptions, node As SyntaxNode, dgml As XElement,
                                        semanticModel As SemanticModel,
                                        Optional ByRef count As Integer = 0,
                                        Optional parent As XElement = Nothing,
                                        Optional parentGroup As XElement = Nothing,
                                        Optional properties As HashSet(Of String) = Nothing)
        Dim operation = semanticModel.GetOperation(node)
        If operation IsNot Nothing Then
            ProcessIOperation(options, operation, dgml, count, parent, parentGroup, properties)
        Else
            ProcessNode(options, node, dgml, semanticModel, count, parent, parentGroup, properties)
        End If
    End Sub

    Private Sub ProcessNode(options As IOperationDgmlOptions, node As SyntaxNode, dgml As XElement,
                            semanticModel As SemanticModel,
                            Optional ByRef count As Integer = 0,
                            Optional parent As XElement = Nothing,
                            Optional parentGroup As XElement = Nothing,
                            Optional properties As HashSet(Of String) = Nothing)
        count += 1

        Dim current = <Node Id=<%= count %> Label=<%= GetLabelForNode(node) %>/>
        Dim currentID = count, parentID = -1, currentGroupID = -1, parentGroupID = -1
        Initialize(options, dgml, parent, parentGroup, current, properties, currentID, parentID, currentGroupID, parentGroupID)
        AddNodeInfo(options, node, current, dgml, properties)
        Dim currentGroup As XElement = parentGroup

        current.@Category = "0"

        If options.ShowGroups Then
            count += 1
            currentGroup = <Node Group="Expanded" Id=<%= count %> Label=<%= GetLabelForNode(node) %>/>
            AddNodeInfo(options, node, currentGroup, dgml, properties)
            dgml.<Nodes>.First.Add(currentGroup)
            currentGroupID = count
            dgml.<Links>.First.Add(<Link Source=<%= currentGroupID %> Target=<%= currentID %> Category="5"></Link>)
            If parentGroupID <> -1 Then
                dgml.<Links>.First.Add(<Link Source=<%= parentGroupID %> Target=<%= currentGroupID %> Category="5"></Link>)
            End If
        End If

        Dim kind = node.GetKind()

        If (node.IsMissing OrElse node.Span.Length = 0) AndAlso Not kind = "CompilationUnit" Then
            current.@Category = "2"
        End If

        If kind.Contains("Bad") OrElse kind.Contains("Skipped") Then
            current.@Category = "3"
        End If

        If options.ShowErrors AndAlso node.ContainsDiagnostics Then
            AddErrorIcon(current)
        End If

        For Each childSyntaxNode In node.ChildNodes()
            ProcessNodeOrIOperation(options, childSyntaxNode, dgml, semanticModel, count, current, currentGroup, properties)
        Next
    End Sub

    Private Sub ProcessIOperation(options As IOperationDgmlOptions, operation As IOperation, dgml As XElement,
                                  Optional ByRef count As Integer = 0,
                                  Optional parent As XElement = Nothing,
                                  Optional parentGroup As XElement = Nothing,
                                  Optional properties As HashSet(Of String) = Nothing)
        count += 1

        Dim current = <Node Id=<%= count %> Label=<%= GetLabelForIOperation(operation) %>/>
        Dim currentID = count, parentID = -1, currentGroupID = -1, parentGroupID = -1
        Initialize(options, dgml, parent, parentGroup, current, properties, currentID, parentID, currentGroupID, parentGroupID)
        AddIOperationInfo(options, operation, current, dgml, properties)
        Dim currentGroup As XElement = parentGroup

        current.@Category = "1"

        If options.ShowGroups Then
            count += 1
            currentGroup = <Node Group="Expanded" Id=<%= count %> Label=<%= GetLabelForIOperation(operation) %>/>
            AddIOperationInfo(options, operation, currentGroup, dgml, properties)
            dgml.<Nodes>.First.Add(currentGroup)
            currentGroupID = count
            dgml.<Links>.First.Add(<Link Source=<%= currentGroupID %> Target=<%= currentID %> Category="5"></Link>)
            If parentGroupID <> -1 Then
                dgml.<Links>.First.Add(<Link Source=<%= parentGroupID %> Target=<%= currentGroupID %> Category="5"></Link>)
            End If
        End If

        If options.ShowErrors AndAlso operation.Syntax.ContainsDiagnostics Then
            AddErrorIcon(current)
        End If

        For Each childIOperation In operation.GetChildren()
            ProcessIOperation(options, childIOperation, dgml, count, current, currentGroup, properties)
        Next
    End Sub
#End Region

#Region "GetLabel*"
    Private Function GetLabelForNode(node As SyntaxNode) As String
        Return node.GetKind()
    End Function

    Private Function GetLabelForIOperation(node As IOperation) As String
        Return node.Kind.ToString()
    End Function
#End Region

#Region "Add*"
    Private Sub AddNodeInfo(options As IOperationDgmlOptions, node As SyntaxNode,
                            current As XElement, dgml As XElement,
                            properties As HashSet(Of String))
        Dim nodeInfo = GetObjectInfo(node)
        AddDgmlProperty("Type", properties, dgml)
        current.@Type = nodeInfo.TypeName
        AddDgmlProperty("Kind", properties, dgml)
        current.@Kind = node.GetKind()

        If options.ShowSpan Then
            AddDgmlProperty("Span", properties, dgml)
            current.@Span = String.Format("{0} Length: {1}",
                                       node.Span.ToString,
                                       node.Span.Length)
            AddDgmlProperty("FullSpan", properties, dgml)
            current.@FullSpan = String.Format("{0} Length: {1}",
                                           node.FullSpan.ToString,
                                           node.FullSpan.Length)
        End If

        For Each field In nodeInfo.PropertyInfos
            Dim name = field.Name
            If Not (name.Contains("Span") OrElse name.Contains("Kind") OrElse name.Contains("Text")) Then
                AddDgmlProperty(name, properties, dgml)
                current.Add(New XAttribute(name, field.Value.ToString))
            End If
        Next

        Dim syntaxTree = node.SyntaxTree
        If syntaxTree IsNot Nothing AndAlso options.ShowErrors Then
            Dim syntaxErrors = syntaxTree.GetDiagnostics(node)
            AddDgmlProperty("Errors", properties, dgml)
            current.@Errors = String.Format("Count: {0}", syntaxErrors.Count)
            For Each syntaxError In syntaxErrors
                current.@Errors &= vbCrLf & syntaxError.ToString(Nothing)
            Next
        End If

        If options.ShowText Then
            AddDgmlProperty("Text", properties, dgml)
            current.@Text = node.ToString()
            AddDgmlProperty("FullText", properties, dgml)
            current.@FullText = node.ToFullString()
        End If
    End Sub

    Private Sub AddIOperationInfo(options As IOperationDgmlOptions, operation As IOperation,
                            current As XElement, dgml As XElement,
                            properties As HashSet(Of String))
        Dim operationInfo = GetObjectInfo(operation)
        AddDgmlProperty("Type", properties, dgml)
        current.@Type = operationInfo.TypeName
        AddDgmlProperty("Kind", properties, dgml)
        current.@Kind = operation.Kind.ToString()

        For Each field In operationInfo.PropertyInfos
            Dim name = field.Name
            If Not (name.Contains("Kind") OrElse name.Contains("Syntax")) Then
                AddDgmlProperty(name, properties, dgml)
                current.Add(New XAttribute(name, field.Value.ToString))
            End If
        Next

        Dim syntax = operation.Syntax
        Dim syntaxTree = syntax.SyntaxTree
        If syntaxTree IsNot Nothing AndAlso options.ShowErrors Then
            Dim syntaxErrors = syntaxTree.GetDiagnostics(syntax)
            AddDgmlProperty("Errors", properties, dgml)
            current.@Errors = String.Format("Count: {0}", syntaxErrors.Count)
            For Each syntaxError In syntaxErrors
                current.@Errors &= vbCrLf & syntaxError.ToString(Nothing)
            Next
        End If

        If options.ShowText Then
            AddDgmlProperty("Text", properties, dgml)
            current.@Text = syntax.ToString()
            AddDgmlProperty("FullText", properties, dgml)
            current.@FullText = syntax.ToFullString()
        End If
    End Sub

#End Region

#Region "Other Helpers"
    Private Sub Initialize(options As IOperationDgmlOptions,
                           dgml As XElement,
                           parent As XElement,
                           parentGroup As XElement,
                           current As XElement,
                           ByRef properties As HashSet(Of String),
                           currentID As Integer,
                           ByRef parentID As Integer,
                           ByRef currentGroupID As Integer,
                           ByRef parentGroupID As Integer)
        dgml.<Nodes>.First.Add(current)

        parentGroupID = -1 : currentGroupID = -1
        parentID = -1

        If parent IsNot Nothing Then
            parentID = CInt(parent.@Id)
        End If

        If options.ShowGroups Then
            If parentGroup IsNot Nothing Then
                parentGroupID = CInt(parentGroup.@Id)
            End If
            currentGroupID = parentGroupID
        End If

        If parentID <> -1 Then
            dgml.<Links>.First.Add(<Link Source=<%= parentID %> Target=<%= currentID %>></Link>)
        End If

        If options.ShowGroups AndAlso parentGroupID <> -1 Then
            dgml.<Links>.First.Add(<Link Source=<%= parentGroupID %> Target=<%= currentID %> Category="5"></Link>)
        End If

        If properties Is Nothing Then
            properties = New HashSet(Of String)
        End If
    End Sub

    Private Function GetDgmlTemplate(options As IOperationDgmlOptions) As XElement
        Dim dgml = <DirectedGraph Background="LightGray">
                       <Categories>
                           <Category Id="0" Label="SyntaxNode"/>
                           <Category Id="1" Label="IOperation"/>
                           <Category Id="2" Label="Missing / Zero-Width"/>
                           <Category Id="3" Label="Bad / Skipped"/>
                           <Category Id="4" Label="Has Diagnostics"/>
                       </Categories>
                       <Nodes>
                       </Nodes>
                       <Links>
                       </Links>
                       <Properties>
                       </Properties>
                       <Styles>
                           <Style TargetType="Node" GroupLabel="SyntaxNode" ValueLabel="Has category">
                               <Condition Expression="HasCategory('0')"/>
                               <Setter Property="Background" Value="Blue"/>
                               <Setter Property="NodeRadius" Value="5"/>
                           </Style>
                           <Style TargetType="Node" GroupLabel="IOperation" ValueLabel="Has category">
                               <Condition Expression="HasCategory('1')"/>
                               <Setter Property="Background" Value="Sienna"/>
                               <Setter Property="FontStyle" Value="Italic"/>
                               <Setter Property="NodeRadius" Value="5"/>
                           </Style>
                           <Style TargetType="Node" GroupLabel="Missing / Zero-Width" ValueLabel="Has category">
                               <Condition Expression="HasCategory('2')"/>
                               <Setter Property="Background" Value="Black"/>
                               <Setter Property="NodeRadius" Value="5"/>
                           </Style>
                           <Style TargetType="Node" GroupLabel="Bad / Skipped" ValueLabel="Has category">
                               <Condition Expression="HasCategory('3')"/>
                               <Setter Property="Background" Value="Red"/>
                               <Setter Property="FontStyle" Value="Bold"/>
                               <Setter Property="NodeRadius" Value="5"/>
                           </Style>
                           <Style TargetType="Node" GroupLabel="Has Diagnostics" ValueLabel="Has category">
                               <Condition Expression="HasCategory('4')"/>
                               <Setter Property="Icon" Value="CodeSchema_Event"/>
                           </Style>
                       </Styles>
                   </DirectedGraph>

        dgml.AddAnnotation(SaveOptions.OmitDuplicateNamespaces)

        If options.ShowGroups Then
            dgml.<Categories>.First.Add(<Category Id="5" Label="Contains" CanBeDataDriven="False" CanLinkedNodesBeDataDriven="True" IncomingActionLabel="Contained By" IsContainment="True" OutgoingActionLabel="Contains"/>)
        End If
        Return dgml
    End Function

    Private Sub AddDgmlProperty(propertyName As String, properties As HashSet(Of String), dgml As XElement)
        If Not properties.Contains(propertyName) Then
            dgml.<Properties>.First.Add(<Property Id=<%= propertyName %> Label=<%= propertyName %> DataType="System.String"/>)
            properties.Add(propertyName)
        End If
    End Sub

    Private Sub AddErrorIcon(element As XElement)
        element.@Icon = "CodeSchema_Event"
    End Sub
#End Region
End Module
