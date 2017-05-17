﻿' Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

Imports Microsoft.CodeAnalysis.VisualBasic.Syntax
Imports Roslyn.Test.Utilities

Namespace Microsoft.CodeAnalysis.VisualBasic.UnitTests.Semantics

    Partial Public Class IOperationTests
        Inherits SemanticModelTestBase

        <Fact, WorkItem(17588, "https://github.com/dotnet/roslyn/issues/17588")>
        Public Sub ObjectCreationWithMemberInitializers()
            Dim source = <![CDATA[
Structure B
    Public Field As Boolean
End Structure

Class F
    Public Field As Integer
    Public Property Property1() As String
    Public Property Property2() As B
End Class

Class C
    Public Sub M1()'BIND:"Public Sub M1()"
        Dim x1 = New F()
        Dim x2 = New F() With {.Field = 2}
        Dim x3 = New F() With {.Property1 = ""}
        Dim x4 = New F() With {.Property1 = "", .Field = 2}
        Dim x5 = New F() With {.Property2 = New B() With {.Field = True}}

        Dim e1 = New F() With {.Property2 = 1}
        Dim e2 = New F() From {""}
    End Sub
End Class]]>.Value

            Dim expectedOperationTree = <![CDATA[
IBlockStatement (9 statements, 7 locals) (OperationKind.BlockStatement, IsInvalid) (Syntax: 'Public Sub  ... End Sub')
  Locals: Local_1: x1 As F
    Local_2: x2 As F
    Local_3: x3 As F
    Local_4: x4 As F
    Local_5: x5 As F
    Local_6: e1 As F
    Local_7: e2 As F
  IVariableDeclarationStatement (1 declarations) (OperationKind.VariableDeclarationStatement) (Syntax: 'Dim x1 = New F()')
    IVariableDeclaration (1 variables) (OperationKind.VariableDeclaration) (Syntax: 'x1')
      Variables: Local_1: x1 As F
      Initializer: IObjectCreationExpression (Constructor: Sub F..ctor()) (OperationKind.ObjectCreationExpression, Type: F) (Syntax: 'New F()')
  IVariableDeclarationStatement (1 declarations) (OperationKind.VariableDeclarationStatement) (Syntax: 'Dim x2 = Ne ... .Field = 2}')
    IVariableDeclaration (1 variables) (OperationKind.VariableDeclaration) (Syntax: 'x2')
      Variables: Local_1: x2 As F
      Initializer: IObjectCreationExpression (Constructor: Sub F..ctor()) (OperationKind.ObjectCreationExpression, Type: F) (Syntax: 'New F() Wit ... .Field = 2}')
          Initializers(1): IAssignmentExpression (OperationKind.AssignmentExpression, Type: System.Int32) (Syntax: '.Field = 2')
              Left: IFieldReferenceExpression: F.Field As System.Int32 (OperationKind.FieldReferenceExpression, Type: System.Int32) (Syntax: 'Field')
                  Instance Receiver: IOperation:  (OperationKind.None) (Syntax: 'New F() Wit ... .Field = 2}')
              Right: ILiteralExpression (Text: 2) (OperationKind.LiteralExpression, Type: System.Int32, Constant: 2) (Syntax: '2')
  IVariableDeclarationStatement (1 declarations) (OperationKind.VariableDeclarationStatement) (Syntax: 'Dim x3 = Ne ... erty1 = ""}')
    IVariableDeclaration (1 variables) (OperationKind.VariableDeclaration) (Syntax: 'x3')
      Variables: Local_1: x3 As F
      Initializer: IObjectCreationExpression (Constructor: Sub F..ctor()) (OperationKind.ObjectCreationExpression, Type: F) (Syntax: 'New F() Wit ... erty1 = ""}')
          Initializers(1): IAssignmentExpression (OperationKind.AssignmentExpression, Type: System.Void) (Syntax: '.Property1 = ""')
              Left: IPropertyReferenceExpression: Property F.Property1 As System.String (OperationKind.PropertyReferenceExpression, Type: System.String) (Syntax: 'Property1')
                  Instance Receiver: IOperation:  (OperationKind.None) (Syntax: 'New F() Wit ... erty1 = ""}')
              Right: ILiteralExpression (OperationKind.LiteralExpression, Type: System.String, Constant: "") (Syntax: '""')
  IVariableDeclarationStatement (1 declarations) (OperationKind.VariableDeclarationStatement) (Syntax: 'Dim x4 = Ne ... .Field = 2}')
    IVariableDeclaration (1 variables) (OperationKind.VariableDeclaration) (Syntax: 'x4')
      Variables: Local_1: x4 As F
      Initializer: IObjectCreationExpression (Constructor: Sub F..ctor()) (OperationKind.ObjectCreationExpression, Type: F) (Syntax: 'New F() Wit ... .Field = 2}')
          Initializers(2): IAssignmentExpression (OperationKind.AssignmentExpression, Type: System.Void) (Syntax: '.Property1 = ""')
              Left: IPropertyReferenceExpression: Property F.Property1 As System.String (OperationKind.PropertyReferenceExpression, Type: System.String) (Syntax: 'Property1')
                  Instance Receiver: IOperation:  (OperationKind.None) (Syntax: 'New F() Wit ... .Field = 2}')
              Right: ILiteralExpression (OperationKind.LiteralExpression, Type: System.String, Constant: "") (Syntax: '""')
            IAssignmentExpression (OperationKind.AssignmentExpression, Type: System.Int32) (Syntax: '.Field = 2')
              Left: IFieldReferenceExpression: F.Field As System.Int32 (OperationKind.FieldReferenceExpression, Type: System.Int32) (Syntax: 'Field')
                  Instance Receiver: IOperation:  (OperationKind.None) (Syntax: 'New F() Wit ... .Field = 2}')
              Right: ILiteralExpression (Text: 2) (OperationKind.LiteralExpression, Type: System.Int32, Constant: 2) (Syntax: '2')
  IVariableDeclarationStatement (1 declarations) (OperationKind.VariableDeclarationStatement) (Syntax: 'Dim x5 = Ne ... ld = True}}')
    IVariableDeclaration (1 variables) (OperationKind.VariableDeclaration) (Syntax: 'x5')
      Variables: Local_1: x5 As F
      Initializer: IObjectCreationExpression (Constructor: Sub F..ctor()) (OperationKind.ObjectCreationExpression, Type: F) (Syntax: 'New F() Wit ... ld = True}}')
          Initializers(1): IAssignmentExpression (OperationKind.AssignmentExpression, Type: System.Void) (Syntax: '.Property2  ... eld = True}')
              Left: IPropertyReferenceExpression: Property F.Property2 As B (OperationKind.PropertyReferenceExpression, Type: B) (Syntax: 'Property2')
                  Instance Receiver: IOperation:  (OperationKind.None) (Syntax: 'New F() Wit ... ld = True}}')
              Right: IObjectCreationExpression (Constructor: Sub B..ctor()) (OperationKind.ObjectCreationExpression, Type: B) (Syntax: 'New B() Wit ... eld = True}')
                  Initializers(1): IAssignmentExpression (OperationKind.AssignmentExpression, Type: System.Boolean) (Syntax: '.Field = True')
                      Left: IFieldReferenceExpression: B.Field As System.Boolean (OperationKind.FieldReferenceExpression, Type: System.Boolean) (Syntax: 'Field')
                          Instance Receiver: IOperation:  (OperationKind.None) (Syntax: 'New B() Wit ... eld = True}')
                      Right: ILiteralExpression (Text: True) (OperationKind.LiteralExpression, Type: System.Boolean, Constant: True) (Syntax: 'True')
  IVariableDeclarationStatement (1 declarations) (OperationKind.VariableDeclarationStatement, IsInvalid) (Syntax: 'Dim e1 = Ne ... perty2 = 1}')
    IVariableDeclaration (1 variables) (OperationKind.VariableDeclaration, IsInvalid) (Syntax: 'e1')
      Variables: Local_1: e1 As F
      Initializer: IObjectCreationExpression (Constructor: Sub F..ctor()) (OperationKind.ObjectCreationExpression, Type: F, IsInvalid) (Syntax: 'New F() Wit ... perty2 = 1}')
          Initializers(1): IAssignmentExpression (OperationKind.AssignmentExpression, Type: System.Void, IsInvalid) (Syntax: '.Property2 = 1')
              Left: IPropertyReferenceExpression: Property F.Property2 As B (OperationKind.PropertyReferenceExpression, Type: B) (Syntax: 'Property2')
                  Instance Receiver: IOperation:  (OperationKind.None) (Syntax: 'New F() Wit ... perty2 = 1}')
              Right: IConversionExpression (ConversionKind.Basic, Implicit) (OperationKind.ConversionExpression, Type: B, IsInvalid) (Syntax: '1')
                  ILiteralExpression (Text: 1) (OperationKind.LiteralExpression, Type: System.Int32, Constant: 1) (Syntax: '1')
  IVariableDeclarationStatement (1 declarations) (OperationKind.VariableDeclarationStatement, IsInvalid) (Syntax: 'Dim e2 = Ne ... ) From {""}')
    IVariableDeclaration (1 variables) (OperationKind.VariableDeclaration, IsInvalid) (Syntax: 'e2')
      Variables: Local_1: e2 As F
      Initializer: IObjectCreationExpression (Constructor: Sub F..ctor()) (OperationKind.ObjectCreationExpression, Type: F, IsInvalid) (Syntax: 'New F() From {""}')
          Initializers(1): IInvalidExpression (OperationKind.InvalidExpression, Type: ?, IsInvalid) (Syntax: '""')
              Children(1): ILiteralExpression (OperationKind.LiteralExpression, Type: System.String, Constant: "") (Syntax: '""')
  ILabelStatement (Label: exit) (OperationKind.LabelStatement) (Syntax: 'End Sub')
  IReturnStatement (OperationKind.ReturnStatement) (Syntax: 'End Sub')
]]>.Value

            Dim expectedDiagnostics = <![CDATA[
BC30311: Value of type 'Integer' cannot be converted to 'B'.
        Dim e1 = New F() With {.Property2 = 1}
                                            ~
BC36718: Cannot initialize the type 'F' with a collection initializer because it is not a collection type.
        Dim e2 = New F() From {""}
                         ~~~~~~~~~
]]>.Value

            VerifyOperationTreeAndDiagnosticsForTest(Of MethodBlockSyntax)(source, expectedOperationTree, expectedDiagnostics)
        End Sub

        <Fact, WorkItem(17588, "https://github.com/dotnet/roslyn/issues/17588")>
        Public Sub ObjectCreationWithCollectionInitializer()
            Dim source = <![CDATA[
Imports System.Collections.Generic

Class C
    Private ReadOnly field As Integer

    Public Sub M1(x As Integer)
        Dim y As Integer = 0
        Dim x1 = New List(Of Integer) From {x, y, field}'BIND:"New List(Of Integer) From {x, y, field}"
    End Sub
End Class]]>.Value

            Dim expectedOperationTree = <![CDATA[
IObjectCreationExpression (Constructor: Sub System.Collections.Generic.List(Of System.Int32)..ctor()) (OperationKind.ObjectCreationExpression, Type: System.Collections.Generic.List(Of System.Int32)) (Syntax: 'New List(Of ... , y, field}')
  Initializers(3): IInvocationExpression ( Sub System.Collections.Generic.List(Of System.Int32).Add(item As System.Int32)) (OperationKind.InvocationExpression, Type: System.Void) (Syntax: 'x')
      Instance Receiver: IOperation:  (OperationKind.None) (Syntax: 'New List(Of ... , y, field}')
      Arguments(1): IArgument (ArgumentKind.Explicit, Matching Parameter: item) (OperationKind.Argument) (Syntax: 'x')
          IParameterReferenceExpression: x (OperationKind.ParameterReferenceExpression, Type: System.Int32) (Syntax: 'x')
    IInvocationExpression ( Sub System.Collections.Generic.List(Of System.Int32).Add(item As System.Int32)) (OperationKind.InvocationExpression, Type: System.Void) (Syntax: 'y')
      Instance Receiver: IOperation:  (OperationKind.None) (Syntax: 'New List(Of ... , y, field}')
      Arguments(1): IArgument (ArgumentKind.Explicit, Matching Parameter: item) (OperationKind.Argument) (Syntax: 'y')
          ILocalReferenceExpression: y (OperationKind.LocalReferenceExpression, Type: System.Int32) (Syntax: 'y')
    IInvocationExpression ( Sub System.Collections.Generic.List(Of System.Int32).Add(item As System.Int32)) (OperationKind.InvocationExpression, Type: System.Void) (Syntax: 'field')
      Instance Receiver: IOperation:  (OperationKind.None) (Syntax: 'New List(Of ... , y, field}')
      Arguments(1): IArgument (ArgumentKind.Explicit, Matching Parameter: item) (OperationKind.Argument) (Syntax: 'field')
          IFieldReferenceExpression: C.field As System.Int32 (OperationKind.FieldReferenceExpression, Type: System.Int32) (Syntax: 'field')
            Instance Receiver: IInstanceReferenceExpression (InstanceReferenceKind.Implicit) (OperationKind.InstanceReferenceExpression, Type: C) (Syntax: 'field')
]]>.Value

            Dim expectedDiagnostics = String.Empty

            VerifyOperationTreeAndDiagnosticsForTest(Of ObjectCreationExpressionSyntax)(source, expectedOperationTree, expectedDiagnostics)
        End Sub

        <Fact, WorkItem(17588, "https://github.com/dotnet/roslyn/issues/17588")>
        Public Sub ObjectCreationWithNestedCollectionInitializer()
            Dim source = <![CDATA[
Imports System.Collections.Generic
Imports System.Linq

Class C
    Private ReadOnly field As Integer

    Public Sub M1(x As Integer)
        Dim y As Integer = 0
        Dim x1 = New List(Of List(Of Integer)) From {{x, y}.ToList, New List(Of Integer) From {field}}'BIND:"New List(Of List(Of Integer)) From {{x, y}.ToList, New List(Of Integer) From {field}}"
    End Sub
End Class]]>.Value

            Dim expectedOperationTree = <![CDATA[
IObjectCreationExpression (Constructor: Sub System.Collections.Generic.List(Of System.Collections.Generic.List(Of System.Int32))..ctor()) (OperationKind.ObjectCreationExpression, Type: System.Collections.Generic.List(Of System.Collections.Generic.List(Of System.Int32))) (Syntax: 'New List(Of ... om {field}}')
  Initializers(2): IInvocationExpression ( Sub System.Collections.Generic.List(Of System.Collections.Generic.List(Of System.Int32)).Add(item As System.Collections.Generic.List(Of System.Int32))) (OperationKind.InvocationExpression, Type: System.Void) (Syntax: '{x, y}.ToList')
      Instance Receiver: IOperation:  (OperationKind.None) (Syntax: 'New List(Of ... om {field}}')
      Arguments(1): IArgument (ArgumentKind.Explicit, Matching Parameter: item) (OperationKind.Argument) (Syntax: '{x, y}.ToList')
          IInvocationExpression ( Function System.Collections.Generic.IEnumerable(Of System.Int32).ToList() As System.Collections.Generic.List(Of System.Int32)) (OperationKind.InvocationExpression, Type: System.Collections.Generic.List(Of System.Int32)) (Syntax: '{x, y}.ToList')
            Instance Receiver: IConversionExpression (ConversionKind.Basic, Implicit) (OperationKind.ConversionExpression, Type: System.Collections.Generic.IEnumerable(Of System.Int32)) (Syntax: '{x, y}')
                IArrayCreationExpression (Element Type: System.Int32) (OperationKind.ArrayCreationExpression, Type: System.Int32()) (Syntax: '{x, y}')
                  Dimension Sizes(1): ILiteralExpression (OperationKind.LiteralExpression, Type: System.Int32, Constant: 2) (Syntax: '{x, y}')
                  Initializer: IArrayInitializer (2 elements) (OperationKind.ArrayInitializer) (Syntax: '{x, y}')
                      Element Values(2): IParameterReferenceExpression: x (OperationKind.ParameterReferenceExpression, Type: System.Int32) (Syntax: 'x')
                        ILocalReferenceExpression: y (OperationKind.LocalReferenceExpression, Type: System.Int32) (Syntax: 'y')
    IInvocationExpression ( Sub System.Collections.Generic.List(Of System.Collections.Generic.List(Of System.Int32)).Add(item As System.Collections.Generic.List(Of System.Int32))) (OperationKind.InvocationExpression, Type: System.Void) (Syntax: 'New List(Of ... rom {field}')
      Instance Receiver: IOperation:  (OperationKind.None) (Syntax: 'New List(Of ... om {field}}')
      Arguments(1): IArgument (ArgumentKind.Explicit, Matching Parameter: item) (OperationKind.Argument) (Syntax: 'New List(Of ... rom {field}')
          IObjectCreationExpression (Constructor: Sub System.Collections.Generic.List(Of System.Int32)..ctor()) (OperationKind.ObjectCreationExpression, Type: System.Collections.Generic.List(Of System.Int32)) (Syntax: 'New List(Of ... rom {field}')
            Initializers(1): IInvocationExpression ( Sub System.Collections.Generic.List(Of System.Int32).Add(item As System.Int32)) (OperationKind.InvocationExpression, Type: System.Void) (Syntax: 'field')
                Instance Receiver: IOperation:  (OperationKind.None) (Syntax: 'New List(Of ... rom {field}')
                Arguments(1): IArgument (ArgumentKind.Explicit, Matching Parameter: item) (OperationKind.Argument) (Syntax: 'field')
                    IFieldReferenceExpression: C.field As System.Int32 (OperationKind.FieldReferenceExpression, Type: System.Int32) (Syntax: 'field')
                      Instance Receiver: IInstanceReferenceExpression (InstanceReferenceKind.Implicit) (OperationKind.InstanceReferenceExpression, Type: C) (Syntax: 'field')
]]>.Value

            Dim expectedDiagnostics = String.Empty

            VerifyOperationTreeAndDiagnosticsForTest(Of ObjectCreationExpressionSyntax)(source, expectedOperationTree, expectedDiagnostics)
        End Sub

        <Fact, WorkItem(17588, "https://github.com/dotnet/roslyn/issues/17588")>
        Public Sub ObjectCreationWithMemberAndCollectionInitializers()
            Dim source = <![CDATA[
Imports System.Collections.Generic

Friend Class [Class]
    Public Property X As Integer
    Public Property Y As Integer()
    Public Property Z As Dictionary(Of Integer, Integer)
    Public Property C As [Class]

    Private ReadOnly field As Integer

    Public Sub M(x As Integer)
        Dim y As Integer = 0
        Dim c = New [Class]() With {'BIND:"New [Class]() With {"
            .X = x,
            .Y = {x, y, 3},
            .Z = New Dictionary(Of Integer, Integer) From {{x, y}},
            .C = New [Class]() With {.X = field}
        }
    End Sub
End Class]]>.Value

            Dim expectedOperationTree = <![CDATA[
IObjectCreationExpression (Constructor: Sub [Class]..ctor()) (OperationKind.ObjectCreationExpression, Type: [Class]) (Syntax: 'New [Class] ... }')
  Initializers(4): IAssignmentExpression (OperationKind.AssignmentExpression, Type: System.Void) (Syntax: '.X = x')
      Left: IPropertyReferenceExpression: Property [Class].X As System.Int32 (OperationKind.PropertyReferenceExpression, Type: System.Int32) (Syntax: 'X')
          Instance Receiver: IOperation:  (OperationKind.None) (Syntax: 'New [Class] ... }')
      Right: IParameterReferenceExpression: x (OperationKind.ParameterReferenceExpression, Type: System.Int32) (Syntax: 'x')
    IAssignmentExpression (OperationKind.AssignmentExpression, Type: System.Void) (Syntax: '.Y = {x, y, 3}')
      Left: IPropertyReferenceExpression: Property [Class].Y As System.Int32() (OperationKind.PropertyReferenceExpression, Type: System.Int32()) (Syntax: 'Y')
          Instance Receiver: IOperation:  (OperationKind.None) (Syntax: 'New [Class] ... }')
      Right: IArrayCreationExpression (Element Type: System.Int32) (OperationKind.ArrayCreationExpression, Type: System.Int32()) (Syntax: '{x, y, 3}')
          Dimension Sizes(1): ILiteralExpression (OperationKind.LiteralExpression, Type: System.Int32, Constant: 3) (Syntax: '{x, y, 3}')
          Initializer: IArrayInitializer (3 elements) (OperationKind.ArrayInitializer) (Syntax: '{x, y, 3}')
              Element Values(3): IParameterReferenceExpression: x (OperationKind.ParameterReferenceExpression, Type: System.Int32) (Syntax: 'x')
                ILocalReferenceExpression: y (OperationKind.LocalReferenceExpression, Type: System.Int32) (Syntax: 'y')
                ILiteralExpression (Text: 3) (OperationKind.LiteralExpression, Type: System.Int32, Constant: 3) (Syntax: '3')
    IAssignmentExpression (OperationKind.AssignmentExpression, Type: System.Void) (Syntax: '.Z = New Di ... om {{x, y}}')
      Left: IPropertyReferenceExpression: Property [Class].Z As System.Collections.Generic.Dictionary(Of System.Int32, System.Int32) (OperationKind.PropertyReferenceExpression, Type: System.Collections.Generic.Dictionary(Of System.Int32, System.Int32)) (Syntax: 'Z')
          Instance Receiver: IOperation:  (OperationKind.None) (Syntax: 'New [Class] ... }')
      Right: IObjectCreationExpression (Constructor: Sub System.Collections.Generic.Dictionary(Of System.Int32, System.Int32)..ctor()) (OperationKind.ObjectCreationExpression, Type: System.Collections.Generic.Dictionary(Of System.Int32, System.Int32)) (Syntax: 'New Diction ... om {{x, y}}')
          Initializers(1): IInvocationExpression ( Sub System.Collections.Generic.Dictionary(Of System.Int32, System.Int32).Add(key As System.Int32, value As System.Int32)) (OperationKind.InvocationExpression, Type: System.Void) (Syntax: '{x, y}')
              Instance Receiver: IOperation:  (OperationKind.None) (Syntax: 'New Diction ... om {{x, y}}')
              Arguments(2): IArgument (ArgumentKind.Explicit, Matching Parameter: key) (OperationKind.Argument) (Syntax: 'x')
                  IParameterReferenceExpression: x (OperationKind.ParameterReferenceExpression, Type: System.Int32) (Syntax: 'x')
                IArgument (ArgumentKind.Explicit, Matching Parameter: value) (OperationKind.Argument) (Syntax: 'y')
                  ILocalReferenceExpression: y (OperationKind.LocalReferenceExpression, Type: System.Int32) (Syntax: 'y')
    IAssignmentExpression (OperationKind.AssignmentExpression, Type: System.Void) (Syntax: '.C = New [C ... .X = field}')
      Left: IPropertyReferenceExpression: Property [Class].C As [Class] (OperationKind.PropertyReferenceExpression, Type: [Class]) (Syntax: 'C')
          Instance Receiver: IOperation:  (OperationKind.None) (Syntax: 'New [Class] ... }')
      Right: IObjectCreationExpression (Constructor: Sub [Class]..ctor()) (OperationKind.ObjectCreationExpression, Type: [Class]) (Syntax: 'New [Class] ... .X = field}')
          Initializers(1): IAssignmentExpression (OperationKind.AssignmentExpression, Type: System.Void) (Syntax: '.X = field')
              Left: IPropertyReferenceExpression: Property [Class].X As System.Int32 (OperationKind.PropertyReferenceExpression, Type: System.Int32) (Syntax: 'X')
                  Instance Receiver: IOperation:  (OperationKind.None) (Syntax: 'New [Class] ... .X = field}')
              Right: IFieldReferenceExpression: [Class].field As System.Int32 (OperationKind.FieldReferenceExpression, Type: System.Int32) (Syntax: 'field')
                  Instance Receiver: IInstanceReferenceExpression (InstanceReferenceKind.Implicit) (OperationKind.InstanceReferenceExpression, Type: [Class]) (Syntax: 'field')
]]>.Value

            Dim expectedDiagnostics = String.Empty

            VerifyOperationTreeAndDiagnosticsForTest(Of ObjectCreationExpressionSyntax)(source, expectedOperationTree, expectedDiagnostics)
        End Sub
    End Class
End Namespace