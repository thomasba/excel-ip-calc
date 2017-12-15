'    Copyright 2010-2017 Thomas Rohmer-Kretz

'    This program is free software: you can redistribute it and/or modify
'    it under the terms of the GNU General Public License as published by
'    the Free Software Foundation, either version 3 of the License, or
'    (at your option) any later version.

'    This program is distributed in the hope that it will be useful,
'    but WITHOUT ANY WARRANTY; without even the implied warranty of
'    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
'    GNU General Public License for more details.

'    You should have received a copy of the GNU General Public License
'    along with this program.  If not, see <http://www.gnu.org/licenses/>.

'    http://trk.free.fr/ipcalc/

'    Visual Basic for Excel

'==============================================
'   IP v4
'==============================================

'----------------------------------------------
'   IpIsValid
'----------------------------------------------
' Returns true if an ip address is formated exactly as it should be:
' no space, no extra zero, no incorrect value
Function IpIsValid(ByVal ip As String) As Boolean
    IpIsValid = (IpBinToStr(IpStrToBin(ip)) = ip)
End Function

'----------------------------------------------
'   IpStrToBin
'----------------------------------------------
' Converts a text IP address to binary
' example:
'   IpStrToBin("1.2.3.4") returns 16909060
Function IpStrToBin(ByVal ip As String) As Double
    Dim pos As Integer
    ip = ip + "."
    IpStrToBin = 0
    While ip <> ""
        pos = InStr(ip, ".")
        IpStrToBin = IpStrToBin * 256 + Val(Left(ip, pos - 1))
        ip = Mid(ip, pos + 1)
    Wend
End Function

'----------------------------------------------
'   IpBinToStr
'----------------------------------------------
' Converts a binary IP address to text
' example:
'   IpBinToStr(16909060) returns "1.2.3.4"
Function IpBinToStr(ByVal ip As Double) As String
    Dim divEnt As Double
    Dim i As Integer
    i = 0
    IpBinToStr = ""
    While i < 4
        If IpBinToStr <> "" Then IpBinToStr = "." + IpBinToStr
        divEnt = Int(ip / 256)
        IpBinToStr = Format(ip - (divEnt * 256)) + IpBinToStr
        ip = divEnt
        i = i + 1
    Wend
End Function

'----------------------------------------------
'   IpSubnetToBin
'----------------------------------------------
' Converts a subnet to binary
' This function is similar to IpStrToBin but ignores the host part of the address
' example:
'   IpSubnetToBin("1.2.3.4/24") returns 16909056
'   IpSubnetToBin("1.2.3.0/24") returns 16909056
Function IpSubnetToBin(ByVal ip As String) As Double
    Dim l As Integer
    Dim pos As Integer
    Dim v As Integer
    l = IpSubnetParse(ip)
    ip = ip + "."
    IpSubnetToBin = 0
    While ip <> ""
        pos = InStr(ip, ".")
        v = Val(Left(ip, pos - 1))
        If (l <= 0) Then
            v = 0
        ElseIf (l < 8) Then
            v = v And ((2 ^ l - 1) * 2 ^ (8 - l))
        End If
        IpSubnetToBin = IpSubnetToBin * 256 + v
        ip = Mid(ip, pos + 1)
        l = l - 8
    Wend
End Function

'----------------------------------------------
'   IpAdd
'----------------------------------------------
' example:
'   IpAdd("192.168.1.1"; 4) returns "192.168.1.5"
'   IpAdd("192.168.1.1"; 256) returns "192.168.2.1"
Function IpAdd(ByVal ip As String, offset As Double) As String
    IpAdd = IpBinToStr(IpStrToBin(ip) + offset)
End Function

'----------------------------------------------
'   IpAnd
'----------------------------------------------
' bitwise AND
' example:
'   IpAnd("192.168.1.1"; "255.255.255.0") returns "192.168.1.0"
Function IpAnd(ByVal ip1 As String, ByVal ip2 As String) As String
    ' compute bitwise AND from right to left
    Dim result As String
    While ((ip1 <> "") And (ip2 <> ""))
        Call IpBuild(IpParse(ip1) And IpParse(ip2), result)
    Wend
    IpAnd = result
End Function

'----------------------------------------------
'   IpOr
'----------------------------------------------
' bitwise OR
' example:
'   IpOr("192.168.1.1"; "0.0.0.255") returns "192.168.1.255"
Function IpOr(ByVal ip1 As String, ByVal ip2 As String) As String
    ' compute bitwise OR from right to left
    Dim result As String
    While ((ip1 <> "") And (ip2 <> ""))
        Call IpBuild(IpParse(ip1) Or IpParse(ip2), result)
    Wend
    IpOr = result
End Function

'----------------------------------------------
'   IpXor
'----------------------------------------------
' bitwise XOR
' example:
'   IpXor("192.168.1.1"; "0.0.0.255") returns "192.168.1.254"
Function IpXor(ByVal ip1 As String, ByVal ip2 As String) As String
    ' compute bitwise XOR from right to left
    Dim result As String
    While ((ip1 <> "") And (ip2 <> ""))
        Call IpBuild(IpParse(ip1) Xor IpParse(ip2), result)
    Wend
    IpXor = result
End Function

'----------------------------------------------
'   IpAdd2
'----------------------------------------------
' another implementation of IpAdd which not use the binary representation
Function IpAdd2(ByVal ip As String, offset As Double) As String
    Dim result As String
    While (ip <> "")
        offset = IpBuild(IpParse(ip) + offset, result)
    Wend
    IpAdd2 = result
End Function

'----------------------------------------------
'   IpComp
'----------------------------------------------
' Compares the first 'n' bits of ip1 and ip2
' example:
'   IpComp("10.0.0.0", "10.1.0.0", 9) returns TRUE
'   IpComp("10.0.0.0", "10.1.0.0", 16) returns FALSE
Function IpComp(ByVal ip1 As String, ByVal ip2 As String, ByVal n As Integer) As Boolean
    Dim pos1 As Integer
    Dim pos2 As Integer
    Dim mask As Integer
    ip1 = ip1 + "."
    ip2 = ip2 + "."
    While (n > 0) And (ip1 <> "") And (ip2 <> "")
        pos1 = InStr(ip1, ".")
        pos2 = InStr(ip2, ".")
        If n >= 8 Then
            If pos1 <> pos2 Then
                IpComp = False
                Exit Function
            End If
            If Left(ip1, pos1) <> Left(ip2, pos2) Then
                IpComp = False
                Exit Function
            End If
        Else
            mask = (2 ^ n - 1) * 2 ^ (8 - n)
            IpComp = ((Val(Left(ip1, pos1 - 1)) And mask) = (Val(Left(ip2, pos2 - 1)) And mask))
            Exit Function
        End If
        n = n - 8
        ip1 = Mid(ip1, pos1 + 1)
        ip2 = Mid(ip2, pos2 + 1)
    Wend
    IpComp = True
End Function

'----------------------------------------------
'   IpGetByte
'----------------------------------------------
' get one byte from an ip address given its position
' example:
'   IpGetByte("192.168.1.1"; 1) returns 192
Function IpGetByte(ByVal ip As String, pos As Integer) As Integer
    pos = 4 - pos
    For i = 0 To pos
        IpGetByte = IpParse(ip)
    Next
End Function

'----------------------------------------------
'   IpSetByte
'----------------------------------------------
' set one byte in an ip address given its position and value
' example:
'   IpSetByte("192.168.1.1"; 4; 20) returns "192.168.1.20"
Function IpSetByte(ByVal ip As String, pos As Integer, newvalue As Integer) As String
    Dim result As String
    Dim byteval As Double
    i = 4
    While (ip <> "")
        byteval = IpParse(ip)
        If (i = pos) Then byteval = newvalue
        Call IpBuild(byteval, result)
        i = i - 1
    Wend
    IpSetByte = result
End Function

'----------------------------------------------
'   IpMask
'----------------------------------------------
' returns an IP netmask from a subnet
' both notations are accepted
' example:
'   IpMask("192.168.1.1/24") returns "255.255.255.0"
'   IpMask("192.168.1.1 255.255.255.0") returns "255.255.255.0"
Function IpMask(ByVal ip As String) As String
    IpMask = IpBinToStr(IpMaskBin(ip))
End Function

'----------------------------------------------
'   IpWildMask
'----------------------------------------------
' returns an IP Wildcard (inverse) mask from a subnet
' both notations are accepted
' example:
'   IpWildMask("192.168.1.1/24") returns "0.0.0.255"
'   IpWildMask("192.168.1.1 255.255.255.0") returns "0.0.0.255"
Function IpWildMask(ByVal ip As String) As String
    IpWildMask = IpBinToStr(((2 ^ 32) - 1) - IpMaskBin(ip))
End Function

'----------------------------------------------
'   IpInvertMask
'----------------------------------------------
' returns an IP Wildcard (inverse) mask from a subnet mask
' or a subnet mask from a wildcard mask
' example:
'   IpInvertMask("255.255.255.0") returns "0.0.0.255"
'   IpInvertMask("0.0.0.255") returns "255.255.255.0"
Function IpInvertMask(ByVal mask As String) As String
    IpInvertMask = IpBinToStr(((2 ^ 32) - 1) - IpStrToBin(mask))
End Function

'----------------------------------------------
'   IpMaskLen
'----------------------------------------------
' returns prefix length from a mask given by a string notation (xx.xx.xx.xx)
' example:
'   IpMaskLen("255.255.255.0") returns 24 which is the number of bits of the subnetwork prefix
Function IpMaskLen(ByVal ipmaskstr As String) As Integer
    Dim notMask As Double
    notMask = 2 ^ 32 - 1 - IpStrToBin(ipmaskstr)
    zeroBits = 0
    Do While notMask <> 0
        notMask = Int(notMask / 2)
        zeroBits = zeroBits + 1
    Loop
    IpMaskLen = 32 - zeroBits
End Function

'----------------------------------------------
'   IpWithoutMask
'----------------------------------------------
' removes the netmask notation at the end of the IP
' example:
'   IpWithoutMask("192.168.1.1/24") returns "192.168.1.1"
'   IpWithoutMask("192.168.1.1 255.255.255.0") returns "192.168.1.1"
Function IpWithoutMask(ByVal ip As String) As String
    Dim p As Integer
    p = InStr(ip, "/")
    If (p = 0) Then
        p = InStr(ip, " ")
    End If
    If (p = 0) Then
        IpWithoutMask = ip
    Else
        IpWithoutMask = Left(ip, p - 1)
    End If
End Function

'----------------------------------------------
'   IpSubnetLen
'----------------------------------------------
' get the mask len from a subnet
' example:
'   IpSubnetLen("192.168.1.1/24") returns 24
'   IpSubnetLen("192.168.1.1 255.255.255.0") returns 24
Function IpSubnetLen(ByVal ip As String) As Integer
    Dim p As Integer
    p = InStr(ip, "/")
    If (p = 0) Then
        p = InStr(ip, " ")
        If (p = 0) Then
            IpSubnetLen = 32
        Else
            IpSubnetLen = IpMaskLen(Mid(ip, p + 1))
        End If
    Else
        IpSubnetLen = Val(Mid(ip, p + 1))
    End If
End Function

'----------------------------------------------
'   IpSubnetParse
'----------------------------------------------
' Get the mask len from a subnet and remove the mask from the address
' The ip parameter is modified and the subnet mask is removed
' example:
'   IpSubnetLen("192.168.1.1/24") returns 24 and ip is changed to "192.168.1.1"
'   IpSubnetLen("192.168.1.1 255.255.255.0") returns 24 and ip is changed to "192.168.1.1"
Function IpSubnetParse(ByRef ip As String) As Integer
    Dim p As Integer
    p = InStr(ip, "/")
    If (p = 0) Then
        p = InStr(ip, " ")
        If (p = 0) Then
            IpSubnetParse = 32
        Else
            IpSubnetParse = IpMaskLen(Mid(ip, p + 1))
            ip = Left(ip, p - 1)
        End If
    Else
        IpSubnetParse = Val(Mid(ip, p + 1))
        ip = Left(ip, p - 1)
    End If
End Function

'----------------------------------------------
'   IpSubnetSize
'----------------------------------------------
' returns the number of addresses in a subnet
' example:
'   IpSubnetSize("192.168.1.32/29") returns 8
'   IpSubnetSize("192.168.1.0 255.255.255.0") returns 256
Function IpSubnetSize(ByVal subnet As String) As Double
    IpSubnetSize = 2 ^ (32 - IpSubnetLen(subnet))
End Function

'----------------------------------------------
'   IpClearHostBits
'----------------------------------------------
' set to zero the bits in the host part of an address
' example:
'   IpClearHostBits("192.168.1.1/24") returns "192.168.1.0/24"
'   IpClearHostBits("192.168.1.193 255.255.255.128") returns "192.168.1.128 255.255.255.128"
Function IpClearHostBits(ByVal net As String) As String
    Dim ip As String
    ip = IpWithoutMask(net)
    IpClearHostBits = IpAnd(ip, IpMask(net)) + Mid(net, Len(ip) + 1)
End Function

'----------------------------------------------
'   IpIsInSubnet
'----------------------------------------------
' Returns TRUE if "ip" is in "subnet"
' example:
'   IpIsInSubnet("192.168.1.35"; "192.168.1.32/29") returns TRUE
'   IpIsInSubnet("192.168.1.35"; "192.168.1.32 255.255.255.248") returns TRUE
'   IpIsInSubnet("192.168.1.41"; "192.168.1.32/29") returns FALSE
Function IpIsInSubnet(ByVal ip As String, ByVal subnet As String) As Boolean
    Dim l As Integer
    l = IpSubnetParse(subnet)
    IpIsInSubnet = IpComp(ip, subnet, l)
End Function

'----------------------------------------------
'   IpSubnetMatch
'----------------------------------------------
' Tries to match an IP address or a subnet against a list of subnets in the
' left-most column of table_array and returns the row number
' 'ip' is the value to search for in the subnets in the first column of
'      the table_array
' 'table_array' is one or more columns of data
' 'fast' indicates the search mode : BestMatch or Fast mode
' fast = 0 (default value)
'    This will work on any subnet list. If the search value matches more
'    than one subnet, the smallest subnet will be returned (best match)
' fast = 1
'    The subnet list MUST be sorted in ascending order and MUST NOT contain
'    overlapping subnets. This mode performs a dichotomic search and runs
'    much faster with large subnet lists.
' The function returns 0 if the IP address is not matched.
Function IpSubnetMatch(ByVal ip As String, table_array As Range, Optional fast As Boolean = False) As Integer
    Dim i As Integer
    IpSubnetMatch = 0
    If fast Then
        Dim a As Integer
        Dim b As Integer
        Dim ip_bin As Double
        a = 1
        b = table_array.Rows.Count
        ip_bin = IpSubnetToBin(ip)
        Do
            i = (a + b + 0.5) / 2
            If ip_bin < IpSubnetToBin(table_array.Cells(i, 1)) Then
                b = i - 1
            Else
                a = i
            End If
        Loop While a < b
        If IpSubnetIsInSubnet(ip, table_array.Cells(a, 1)) Then
            IpSubnetMatch = a
        End If
    Else
        Dim previousMatchLen As Integer
        Dim searchLen As Integer
        Dim subnet As String
        Dim subnetLen As Integer
        searchLen = IpSubnetParse(ip)
        previousMatchLen = 0
        For i = 1 To table_array.Rows.Count
            subnet = table_array.Cells(i, 1)
            subnetLen = IpSubnetParse(subnet)
            If subnetLen > previousMatchLen Then
                If searchLen >= subnetLen Then
                    If IpComp(ip, subnet, subnetLen) Then
                        previousMatchLen = subnetLen
                        IpSubnetMatch = i
                    End If
                End If
            End If
        Next i
    End If
End Function

'----------------------------------------------
'   IpSubnetVLookup
'----------------------------------------------
' Tries to match an IP address or a subnet against a list of subnets in the
' left-most column of table_array and returns the value in the same row based
' on the index_number
' 'ip' is the value to search for in the subnets in the first column of
'      the table_array
' 'table_array' is one or more columns of data
' 'index_number' is the column number in table_array from which the matching
'      value must be returned. The first column which contains subnets is 1.
' 'fast' indicates the search mode : BestMatch or Fast mode
' fast = 0 (default value)
'    This will work on any subnet list. If the search value matches more
'    than one subnet, the smallest subnet will be returned (best match)
' fast = 1
'    The subnet list MUST be sorted in ascending order and MUST NOT contain
'    overlapping subnets. This mode performs a dichotomic search and runs
'    much faster with large subnet lists.
' Note: add 0.0.0.0/0 in the array if you want the function to return a
' default value (best match mode only)
Function IpSubnetVLookup(ByVal ip As String, table_array As Range, index_number As Integer, Optional fast As Boolean = False) As String
    Dim i As Integer
    i = IpSubnetMatch(ip, table_array, fast)
    If i = 0 Then
        IpSubnetVLookup = "Not Found"
    Else
        IpSubnetVLookup = table_array.Cells(i, index_number)
    End If
End Function

'----------------------------------------------
'   IpSubnetVLookupAreas
'----------------------------------------------
' Same as IpSubnetVLookup except that table_array parameter can be a
' named area containing multiple tables. Use it if you want to search in
' more than one table.
' Doesn't have the 'fast' option.
Function IpSubnetVLookupAreas(ByVal ip As String, table_array As Range, index_number As Integer) As String
    Dim previousMatch As String
    previousMatch = "0.0.0.0/0"
    IpSubnetVLookupAreas = "Not Found"
    For a = 1 To table_array.Areas.Count
        For i = 1 To table_array.Areas(a).Rows.Count
            Dim subnet As String
            subnet = table_array.Areas(a).Cells(i, 1)
            If IpIsInSubnet(ip, subnet) And (IpSubnetLen(subnet) > IpSubnetLen(previousMatch)) Then
                previousMatch = subnet
                IpSubnetVLookupAreas = table_array.Areas(a).Cells(i, index_number)
            End If
        Next i
    Next a
End Function

'----------------------------------------------
'   IpSubnetIsInSubnet
'----------------------------------------------
' Returns TRUE if "subnet1" is in "subnet2"
' example:
'   IpSubnetIsInSubnet("192.168.1.35/30"; "192.168.1.32/29") returns TRUE
'   IpSubnetIsInSubnet("192.168.1.41/30"; "192.168.1.32/29") returns FALSE
'   IpSubnetIsInSubnet("192.168.1.35/28"; "192.168.1.32/29") returns FALSE
'   IpSubnetIsInSubnet("192.168.0.128 255.255.255.128"; "192.168.0.0 255.255.255.0") returns TRUE
Function IpSubnetIsInSubnet(ByVal subnet1 As String, ByVal subnet2 As String) As Boolean
    Dim l1 As Integer
    Dim l2 As Integer
    l1 = IpSubnetParse(subnet1)
    l2 = IpSubnetParse(subnet2)
    If l1 < l2 Then
        IpSubnetIsInSubnet = False
    Else
        IpSubnetIsInSubnet = IpComp(subnet1, subnet2, l2)
    End If
End Function

'----------------------------------------------
'   IpFindOverlappingSubnets
'----------------------------------------------
' this function must be used in an array formula
' it will find in the list of subnets which subnets overlap
' 'SubnetsArray' is single column array containing a list of subnets, the
' list may be sorted or not
' the return value is also a array of the same size
' if the subnet on line x is included in a larger subnet from another line,
' this function returns an array in which line x contains the value of the
' larger subnet
' if the subnet on line x is distinct from any other subnet in the array,
' then this function returns on line x an empty cell
' if there are no overlapping subnets in the input array, the returned array
' is empty
Function IpFindOverlappingSubnets(subnets_array As Range) As Variant
    Dim result_array() As Variant
    ReDim result_array(1 To subnets_array.Rows.Count, 1 To 1)
    For i = 1 To subnets_array.Rows.Count
        result_array(i, 1) = ""
        For j = 1 To subnets_array.Rows.Count
            If (i <> j) And IpSubnetIsInSubnet(subnets_array.Cells(i, 1), subnets_array.Cells(j, 1)) Then
                result_array(i, 1) = subnets_array.Cells(j, 1)
                Exit For
            End If
        Next j
    Next i
    IpFindOverlappingSubnets = result_array
End Function

'----------------------------------------------
'   IpSortArray
'----------------------------------------------
' this function must be used in an array formula
' 'ip_array' is a single column array containing ip addresses
' the return value is also a array of the same size containing the same
' addresses sorted in ascending or descending order
' 'descending' is an optional parameter, if set to True the adresses are
' sorted in descending order
Function IpSortArray(ip_array As Range, Optional descending As Boolean = False) As Variant
    Dim s As Integer
    Dim t As Integer
    t = 0
    s = ip_array.Rows.Count
    Dim list() As Double
    ReDim list(1 To s)
    ' copy the IP list as binary values
    For i = 1 To s
        If (ip_array.Cells(i, 1) <> 0) Then
            t = t + 1
            list(t) = IpStrToBin(ip_array.Cells(i, 1))
        End If
    Next i
    ' sort the list with bubble sort
    For i = t - 1 To 1 Step -1
        For j = 1 To i
            If ((list(j) > list(j + 1)) Xor descending) Then
                Dim swap As Double
                swap = list(j)
                list(j) = list(j + 1)
                list(j + 1) = swap
            End If
        Next j
    Next i
    ' copy the sorted list as strings
    Dim resultArray() As Variant
    ReDim resultArray(1 To s, 1 To 1)
    For i = 1 To t
        resultArray(i, 1) = IpBinToStr(list(i))
    Next i
    IpSortArray = resultArray
End Function

'----------------------------------------------
'   IpSubnetSortArray
'----------------------------------------------
' this function must be used in an array formula
' 'ip_array' is a single column array containing ip subnets in "prefix/len"
' or "prefix mask" notation
' the return value is also an array of the same size containing the same
' subnets sorted in ascending or descending order
' 'descending' is an optional parameter, if set to True the subnets are
' sorted in descending order
Function IpSubnetSortArray(ip_array As Range, Optional descending As Boolean = False) As Variant
    Dim s As Integer
    Dim t As Integer
    t = 0
    s = ip_array.Rows.Count
    Dim list() As String
    ReDim list(1 To s)
    ' copy the IP list as binary values
    For i = 1 To s
        If (ip_array.Cells(i, 1) <> 0) Then
            t = t + 1
            list(t) = ip_array.Cells(i, 1)
        End If
    Next i
    ' sort the list with bubble sort
    For i = t - 1 To 1 Step -1
        For j = 1 To i
            Dim m, n As Double
            m = IpStrToBin(list(j))
            n = IpStrToBin(list(j + 1))
            If (((m > n) Or ((m = n) And (IpMaskBin(list(j)) < IpMaskBin(list(j + 1))))) Xor descending) Then
                Dim swap As String
                swap = list(j)
                list(j) = list(j + 1)
                list(j + 1) = swap
            End If
        Next j
    Next i
    ' copy the sorted list as strings
    Dim resultArray() As Variant
    ReDim resultArray(1 To s, 1 To 1)
    For i = 1 To t
        resultArray(i, 1) = list(i)
    Next i
    IpSubnetSortArray = resultArray
End Function

'----------------------------------------------
'   IpParseRoute
'----------------------------------------------
' this function is used by IpSubnetSortJoinArray to extract the subnet
' and next hop in route
' the supported formats are
' 10.0.0.0 255.255.255.0 1.2.3.4
' 10.0.0.0/24 1.2.3.4
' the next hop can be any character sequence, and not only an IP
Function IpParseRoute(ByVal route As String, ByRef nexthop As String)
    slash = InStr(route, "/")
    sp = InStr(route, " ")
    If ((slash = 0) And (sp > 0)) Then
        temp = Mid(route, sp + 1)
        sp = InStr(sp + 1, route, " ")
    End If
    If (sp = 0) Then
        IpParseRoute = route
        nexthop = ""
    Else
        IpParseRoute = Left(route, sp - 1)
        nexthop = Mid(route, sp + 1)
    End If
End Function

'----------------------------------------------
'   IpSubnetSortJoinArray
'----------------------------------------------
' this function can sort and summarize subnets or ip routes
' it must be used in an array formula
' 'ip_array' is a single column array containing ip subnets in "prefix/len"
' or "prefix mask" notation
' the return value is also an array of the same size containing the same
' subnets sorted in ascending order
' any consecutive subnets of the same size will be summarized when it is
' possible
' each line may contain any character sequence after the subnet, such as
' a next hop or any parameter of an ip route
' in this case, only subnets with the same parameters will be summarized
Function IpSubnetSortJoinArray(ip_array As Range) As Variant
    Dim s As Integer
    Dim t As Integer
    Dim a As String
    Dim b As String
    Dim nexthop1 As String
    Dim nexthop2 As String
    t = 0
    s = ip_array.Rows.Count
    Dim list() As String
    ReDim list(1 To s)
    ' copy subnet list
    For i = 1 To s
        If (ip_array.Cells(i, 1) <> 0) Then
            t = t + 1
            ' just use the networks as provide:
            'list(t) = ip_array.Cells(i, 1)
            ' or clean up the host part in each subnet:
            a = IpParseRoute(ip_array.Cells(i, 1), nexthop1)
            list(t) = IpClearHostBits(a) + " " + nexthop1
        End If
    Next i
    ' sort the list with bubble sort
    For i = t - 1 To 1 Step -1
        For j = 1 To i
            Dim m, n As Double
            a = IpParseRoute(list(j), nexthop1)
            b = IpParseRoute(list(j + 1), nexthop2)
            m = IpStrToBin(IpWithoutMask(a))
            n = IpStrToBin(IpWithoutMask(b))
            If ((m > n) Or ((m = n) And (IpMaskBin(a) < IpMaskBin(b)))) Then
                Dim swap As String
                swap = list(j)
                list(j) = list(j + 1)
                list(j + 1) = swap
            End If
        Next j
    Next i
    ' try to join subnets
    i = 1
    While (i < t)
        remove_next = False
        a = IpParseRoute(list(i), nexthop1)
        b = IpParseRoute(list(i + 1), nexthop2)
        If (IpSubnetIsInSubnet(a, b) And (nexthop1 = nexthop2)) Then
            list(i) = list(i + 1)
            remove_next = True
        ElseIf (IpSubnetIsInSubnet(b, a) And (nexthop1 = nexthop2)) Then
            remove_next = True
        ElseIf ((IpSubnetLen(a) = IpSubnetLen(b)) And (nexthop1 = nexthop2)) Then
            ' create a subnet with the same notation
            bigsubnet = Replace(IpWithoutMask(a) + "/" + Str(IpSubnetLen(a) - 1), " ", "")
            If (InStr(a, "/") = 0) Then
                bigsubnet = IpWithoutMask(a) & " " & IpMask(bigsubnet)
            Else
            End If
            If (IpSubnetIsInSubnet(b, bigsubnet)) Then
                ' OK these subnets can be joined
                list(i) = bigsubnet & " " & nexthop1
                remove_next = True
            End If
        End If
        
        If (remove_next) Then
            ' remove list(i+1) and make the list one element shorter
            For j = i + 1 To t - 1
                list(j) = list(j + 1)
            Next j
            t = t - 1
            ' step back and try again because list(i) may be joined with list(i-1)
            If (i > 1) Then i = i - 1
        Else
            i = i + 1
        End If
    Wend
    ' copy the sorted list as strings
    Dim resultArray() As Variant
    ReDim resultArray(1 To s, 1 To 1)
    For i = 1 To t
        resultArray(i, 1) = list(i)
    Next i
    IpSubnetSortJoinArray = resultArray
End Function

'----------------------------------------------
'   IpDivideSubnet
'----------------------------------------------
' divide a network in smaller subnets
' "n" is the value that will be added to the subnet length
' "SubnetSeqNbr" is the index of the smaller subnet to return
' example:
'   IpDivideSubnet("1.2.3.0/24"; 2; 0) returns "1.2.3.0/26"
'   IpDivideSubnet("1.2.3.0/24"; 2; 1) returns "1.2.3.64/26"
Function IpDivideSubnet(ByVal subnet As String, n As Integer, index As Integer)
    Dim ip As String
    Dim slen As Integer
    ip = IpAnd(IpWithoutMask(subnet), IpMask(subnet))
    slen = IpSubnetLen(subnet) + n
    If (slen > 32) Then
        IpDivideSubnet = "ERR subnet lenght > 32"
        Exit Function
    End If
    If (index >= 2 ^ n) Then
        IpDivideSubnet = "ERR index out of range"
        Exit Function
    End If
    ip = IpBinToStr(IpStrToBin(ip) + (2 ^ (32 - slen)) * index)
    IpDivideSubnet = Replace(ip + "/" + Str(slen), " ", "")
End Function

'----------------------------------------------
'   IpIsPrivate
'----------------------------------------------
' returns TRUE if "ip" is in one of the private IP address ranges
' example:
'   IpIsPrivate("192.168.1.35") returns TRUE
'   IpIsPrivate("209.85.148.104") returns FALSE
Function IpIsPrivate(ByVal ip As String) As Boolean
    IpIsPrivate = (IpIsInSubnet(ip, "10.0.0.0/8") Or IpIsInSubnet(ip, "172.16.0.0/12") Or IpIsInSubnet(ip, "192.168.0.0/16"))
End Function

'----------------------------------------------
'   IpRangeToCIDR
'----------------------------------------------
' returns a network or a list of networks given the first and the
' last address of an IP range
' if this function is used in a array formula, it may return more
' than one network
' example:
'   IpRangeToCIDR("10.0.0.1","10.0.0.254") returns 10.0.0.0/24
'   IpRangeToCIDR("10.0.0.1","10.0.1.63") returns the array : 10.0.0.0/24 10.0.1.0/26
' note:
'   10.0.0.0 or 10.0.0.1 as the first address returns the same result
'   10.0.0.254 or 10.0.0.255 (broadcast) as the last address returns the same result
Function IpRangeToCIDR(ByVal firstAddr As String, ByVal lastAddr As String) As Variant
    firstAddr = IpAnd(firstAddr, "255.255.255.254") ' set the last bit to zero
    lastAddr = IpOr(lastAddr, "0.0.0.1") ' set the last bit to one
    Dim list() As String
    n = 0
    Do
        l = 0
        Do ' find the largest network which first address is firstAddr and which last address is not higher than lastAddr
            ' build a network of length l
            ' if it does not comply the above conditions, try with a smaller network
            l = l + 1
            net = firstAddr & "/" & l
            ip1 = IpAnd(firstAddr, IpMask(net)) ' first @ of this network
            ip2 = IpOr(firstAddr, IpWildMask(net)) ' last @ of this network
            net = ip1 & "/" & l ' rebuild the network with the first address
            diff = IpDiff(ip2, lastAddr) ' difference between the last @ of this network and the lastAddr we need to reach
        Loop While (l < 32) And ((ip1 <> firstAddr) Or (diff > 0))
        
        n = n + 1
        ReDim Preserve list(1 To n)
        list(n) = net
        firstAddr = IpAdd(ip2, 1)
    Loop While (diff < 0) ' if we haven't reached the lastAddr, loop to build another network
    
    Dim resultArray() As Variant
    ReDim resultArray(1 To n + 1, 1 To 1)
    For i = 1 To n
        resultArray(i, 1) = list(i)
    Next i
    IpRangeToCIDR = resultArray
End Function

'----------------------------------------------
'   IpSubtractSubnets
'----------------------------------------------
' Remove subnets from a list of subnets
' this function must be used in an array formula
' 'input_array' is a list of assigned subnets
' 'subtract_array' is a list of used subnets
' the result is a list of unused subnets
Function IpSubtractSubnets(input_array As Range, subtract_array As Range) As Variant
    Dim i As Integer
    Dim j As Integer
    Dim k As Integer
    Dim s As Integer
    s = 0
    Dim list() As String
    ReDim list(1 To input_array.Rows.Count)
    ' copy subnet list
    For i = 1 To input_array.Rows.Count
        If (input_array.Cells(i, 1) <> 0) Then
            s = s + 1
            list(s) = input_array.Cells(i, 1)
        End If
    Next i

    For i = 1 To subtract_array.Rows.Count
        ' try to remove each network in subtract_array from the list in input_array
        subtractNet = subtract_array.Cells(i, 1)
        If subtractNet <> 0 Then
            ' try to remove each network in subtract_array from each network in input_array
            j = 1
            Do
                net = list(j)
                ' is the network to remove equal or larger ?
                If IpSubnetIsInSubnet(net, subtractNet) Then ' remove the network from input_array
                    For k = j To s - 1
                        list(k) = list(k + 1)
                    Next k
                    s = s - 1
                ' is the network to remove smaller ?
                ElseIf IpSubnetIsInSubnet(subtractNet, net) Then ' split this network in input_array
                    ' insert a line in the result array
                    s = s + 1
                    ReDim Preserve list(1 To s)
                    For k = s To j + 2 Step -1
                        list(k) = list(k - 1)
                    Next k
                    ' create 2 smaller subnets
                    list(j + 1) = IpDivideSubnet(list(j), 1, 1)
                    list(j) = IpDivideSubnet(list(j), 1, 0)
                Else
                    ' nothing to do, skip to next network in input_array
                    j = j + 1
                End If
            Loop While j <= s
        End If
    Next i

    Dim resultArray() As Variant
    ReDim resultArray(1 To s + 1, 1 To 1)
    For i = 1 To s
        resultArray(i, 1) = list(i)
    Next i
    IpSubtractSubnets = resultArray
End Function

'----------------------------------------------
'   IpDiff
'----------------------------------------------
' difference between 2 IP addresses
' example:
'   IpDiff("192.168.1.7"; "192.168.1.1") returns 6
Function IpDiff(ByVal ip1 As String, ByVal ip2 As String) As Double
    Dim mult As Double
    mult = 1
    IpDiff = 0
    While ((ip1 <> "") Or (ip2 <> ""))
        IpDiff = IpDiff + mult * (IpParse(ip1) - IpParse(ip2))
        mult = mult * 256
    Wend
End Function

'----------------------------------------------
'   IpParse
'----------------------------------------------
' Parses an IP address by iteration from right to left
' Removes one byte from the right of "ip" and returns it as an integer
' example:
'   if ip="192.168.1.32"
'   IpParse(ip) returns 32 and ip="192.168.1" when the function returns
Function IpParse(ByRef ip As String) As Integer
    Dim pos As Integer
    pos = InStrRev(ip, ".")
    If pos = 0 Then
        IpParse = Val(ip)
        ip = ""
    Else
        IpParse = Val(Mid(ip, pos + 1))
        ip = Left(ip, pos - 1)
    End If
End Function

'----------------------------------------------
'   IpBuild
'----------------------------------------------
' Builds an IP address by iteration from right to left
' Adds "ip_byte" to the left the "ip"
' If "ip_byte" is greater than 255, only the lower 8 bits are added to "ip"
' and the remaining bits are returned to be used on the next IpBuild call
' example 1:
'   if ip="168.1.1"
'   IpBuild(192, ip) returns 0 and ip="192.168.1.1"
' example 2:
'   if ip="1"
'   IpBuild(258, ip) returns 1 and ip="2.1"
Function IpBuild(ip_byte As Double, ByRef ip As String) As Double
    If ip <> "" Then ip = "." + ip
    ip = Format(ip_byte And 255) + ip
    IpBuild = ip_byte \ 256
End Function

'----------------------------------------------
'   IpMaskBin
'----------------------------------------------
' returns binary IP mask from an address with / notation (xx.xx.xx.xx/yy)
' example:
'   IpMask("192.168.1.1/24") returns 4294967040 which is the binary
'   representation of "255.255.255.0"
Function IpMaskBin(ByVal ip As String) As Double
    Dim bits As Integer
    bits = IpSubnetLen(ip)
    IpMaskBin = (2 ^ bits - 1) * 2 ^ (32 - bits)
End Function

'==============================================
'   IP v6
'==============================================

'----------------------------------------------
'   Ipv6MaskLen
'----------------------------------------------
' returns prefix length from an IPv6 net
' example:
'   Ipv6MaskLen("2001:db8:1f89::/48") returns 48
Function Ipv6MaskLen(ByVal CIDRNet As String) As Integer
    slash = InStr(CIDRNet, "/")
    If (slash = 0) Then
        Ipv6MaskLen = 128
    Else
        Ipv6MaskLen = Val(Mid(CIDRNet, slash + 1))
    End If
End Function

'----------------------------------------------
'   Ipv6WithoutMask
'----------------------------------------------
' removes the /xx netmask notation at the end of the IP
' example:
'   Ipv6WithoutMask("2001:db8:1f89::/48") returns "2001:db8:1f89::"
Function Ipv6WithoutMask(ByVal CIDRNet As String) As String
    slash = InStr(CIDRNet, "/")
    If (slash = 0) Then
        Ipv6WithoutMask = CIDRNet
    Else
        Ipv6WithoutMask = Left(CIDRNet, slash - 1)
    End If
End Function

'----------------------------------------------
'   Ipv6IsInSubnet
'----------------------------------------------
' returns TRUE if "ip" is in "subnet"
' example:
'   Ipv6IsInSubnet("2001:db8:1:::ac1f:1"; "2001:db8:1::/48") returns TRUE
'   Ipv6IsInSubnet("2001:db8:2:::ac1f:1"; "2001:db8:1::/48") returns FALSE
Function Ipv6IsInSubnet(ByVal ip As String, ByVal subnet As String) As Variant
    prefixlen = Ipv6MaskLen(subnet)
    subnet = Ipv6ToBin(subnet)
    ip = Ipv6ToBin(ip)
    If (Left(subnet, prefixlen) = Left(ip, prefixlen)) Then
        Ipv6IsInSubnet = True
    Else
        Ipv6IsInSubnet = False
    End If
End Function

'----------------------------------------------
'   Ipv6AddMissingColumns
'----------------------------------------------
' this function is called from Ipv6Expand and replaces the :: by the
' right amount of :
' examples:
'   Ipv6AddMissingColumns(1:2:3::8) returns "1:2:3:::::8"
'   Ipv6AddMissingColumns(1:2:3:4:5::8) returns "1:2:3:4:5:::8"
'   Ipv6AddMissingColumns(1:2:3::) returns "1:2:3:::::"
Function Ipv6AddMissingColumns(ByVal ip As String) As Variant
    d = 0 ' number of double columns
    c = 0 ' number of columns
    For i = 1 To Len(ip)
        If (Mid(ip, i, 2) = "::") Then d = d + 1
        If (Mid(ip, i, 1) = ":") Then c = c + 1
    Next
    If ((d = 0) And (c = 7)) Then
        ' 7 single columns, nothing to do
        ip2 = ip
    ElseIf (d = 1) Then
        ' one double columns, replace with the right number of columns
        ip2 = Replace(ip, "::", Left("::::::::", 9 - c))
    Else
        ' any other cas is an error
        Ipv6AddMissingColumns = CVErr(xlErrValue)
        Exit Function
    End If
    Ipv6AddMissingColumns = ip2
End Function

'----------------------------------------------
'   Ipv6Expand
'----------------------------------------------
' returns a representation of an IPv6 address with all the missing zeros
' the result has a fixed lenght of 39 caracters
' example :
'   Ipv6Expand("1:2:3::8") returns "0001:0002:0003:0000:0000:0000:0000:0008"
Function Ipv6Expand(ByVal ip As String) As Variant
    ip = "0" & Ipv6AddMissingColumns(Ipv6WithoutMask(ip))
    While (ip <> "")
        ip2 = Ipv6Parse(ip) & ip2
        If (ip <> "") Then
            ip2 = ":" & ip2
        End If
    Wend
    Ipv6Expand = ip2
End Function

'----------------------------------------------
'   Ipv6Compress
'----------------------------------------------
' returns the shortest representation of an IPv6 address
' examples:
'   Ipv6Compress("0001:0002:0003:0000:0000:0000:0000:0008") returns "1:2:3::8"
'   Ipv6Compress("01:0:0::") returns "1::"
Function Ipv6Compress(ByVal ip As String) As String
    Dim ip2 As String, ip3 As String, ip4 As String
    
    ' start with the expanded representation of ip
    ip2 = Ipv6Expand(ip)
    ' rebuild ip, this will remove zeros at the begining of each hex block
    ' if a block is null, this will keep one zero
    While (ip2 <> "")
        offset = Ipv6Build(Ipv6ParseInt(ip2), ip3)
    Wend

    ' try to replace the longuest sequence of zero blocks by ::
    s = ":0:0:0:0:0:0:"
    For i = Len(s) To 3 Step -2
        ip4 = Replace(ip3, Left(s, i), "::", 1, 1)
        If (ip3 <> ip4) Then Exit For
    Next
    
    ' remove first 0 if ip starts with 0::
    If (Left(ip4, 3) = "0::") Then ip4 = Mid(ip4, 2)
    ' remove last 0 if ip ends with ::0
    If (Right(ip4, 3) = "::0") Then ip4 = Left(ip4, Len(ip4) - 1)

    Ipv6Compress = ip4
End Function

'----------------------------------------------
'   Ipv6ToBin
'----------------------------------------------
' returns a string representing the binary value of IPv6 address
' the result has a fixed lenght of 128 characters
Function Ipv6ToBin(ByVal ip As String) As Variant
    Dim result As String
    ip2 = Replace(Ipv6Expand(ip), ":", "")
    For i = 1 To Len(ip2)
        b = "0000"
        j = 0
        v = Val("&H" & Mid$(ip2, i, 1))
        While v > 0
            Mid$(b, 4 - j, 1) = v Mod 2
            v = v \ 2
            j = j + 1
        Wend
        result = result & b
    Next
    Ipv6ToBin = result
End Function

'----------------------------------------------
'   Ipv6FromBin
'----------------------------------------------
' returns an IPv6 from a string representing the binary value of IPv6 address
' the parameter must be a 128 character string
Function Ipv6FromBin(ByVal ipbin As String) As Variant
    Dim result As String
    Dim pos As Integer
    pos = 1
    If Len(ipbin) <> 128 Then
        Ipv6FromBin = ""
        Exit Function
    End If
    
    For bloc = 1 To 8
        Dim v As Double
        v = 0
        For bit = 1 To 16
            v = v * 2 + Val(Mid(ipbin, pos, 1))
            pos = pos + 1
        Next
        result = result + LCase(Hex(v))
        If (bloc < 8) Then result = result + ":"
    Next
    Ipv6FromBin = Ipv6Compress(result)
End Function

'----------------------------------------------
'   Ipv6Parse
'----------------------------------------------
' Parses an IPv6 address by iteration from right to left
' Removes a 16-bit value from the right of "ip" and returns it as 4 character
' long hex value
' Important: This function does not expand the :: if there is any
' example:
'   if ip="1:2:3:4:5:6:7:8"
'   IpParse(ip) returns "0008" and ip="1:2:3:4:5:6:7" when the function returns
Function Ipv6Parse(ByRef ip As String) As String
    Dim pos As Integer
    pos = InStrRev(ip, ":")
    If pos = 0 Then
        v = ip
        ip = ""
    Else
        v = Mid(ip, pos + 1)
        ip = Left(ip, pos - 1)
    End If
    Ipv6Parse = Right("0000" & v, 4)
End Function

'----------------------------------------------
'   Ipv6ParseInt
'----------------------------------------------
' Same as Ipv6Parse but returns a Double instead of String
Function Ipv6ParseInt(ByRef ip As String) As Double
    Dim v As Double
    strHex = Ipv6Parse(ip)
    For i = 1 To Len(strHex)
        v = 16 * v + Val("&H" & Mid$(strHex, i, 1))
    Next
    Ipv6ParseInt = v
End Function

'----------------------------------------------
'   Ipv6Build
'----------------------------------------------
' Builds an IP address by iteration from right to left
' Adds "v16bits" to the left the "ip"
' If "v16bits" is greater than 65535 (= FFFF), only the lower 16 bits are
' added to "ip" and the remaining bits are returned to be used on the next
' IpBuild call
Function Ipv6Build(v16bits As Double, ByRef ip As String) As Double
    If ip <> "" Then ip = ":" + ip
    ip = LCase(Hex(v16bits And 65535)) + ip
    Ipv6Build = v16bits \ 65536
End Function

'----------------------------------------------
'   Ipv6AddInt
'----------------------------------------------
' Add a value to an IPv6 address
' example:
'   Ipv6AddInt("1::2"; 16) returns "1:12"
Function Ipv6AddInt(ByVal ip As String, offset As Double) As String
    Dim result As String
    ip = Ipv6Expand(ip)
    While (ip <> "")
        offset = Ipv6Build(Ipv6ParseInt(ip) + offset, result)
    Wend
    Ipv6AddInt = Ipv6Compress(result)
End Function

'----------------------------------------------
'   Ipv6Add
'----------------------------------------------
' Add two IPv6 addresses
' example:
'   Ipv6Add("1:2::"; "::3") returns "1:2::3"
'   Ipv6Add("1:2::2"; "::3") returns "1:2::5"
Function Ipv6Add(ByVal ip1 As String, ByVal ip2 As String) As String
    Dim result As String
    Dim offset As Double
    ip1 = Ipv6Expand(ip1)
    ip2 = Ipv6Expand(ip2)
    While ((ip1 <> "") And (ip2 <> ""))
        offset = Ipv6Build(Ipv6ParseInt(ip1) + Ipv6ParseInt(ip2) + offset, result)
    Wend
    Ipv6Add = Ipv6Compress(result)
End Function

'----------------------------------------------
'   Ipv6GetBlock
'----------------------------------------------
' Returns the 4-digit hexa block at position blockNbr
' The value of blockNbr can be 1 to 8, block 1 is the block on the left.
' example:
'   Ipv6GetBlock("2001:db8:1f89:c5a3::ac1f:8001"; 2) returns "0db8"
Function Ipv6GetBlock(ByVal ip As String, blockNbr As Integer) As String
    Ipv6GetBlock = Mid(Ipv6Expand(ip), blockNbr * 5 - 4, 4)
End Function

'----------------------------------------------
'   Ipv6GetBlockInt
'----------------------------------------------
' Same as above except that the returned value is an integer between
' 0 and 65535
Function Ipv6GetBlockInt(ByVal ip As String, blockNbr As Integer) As Double
    Ipv6GetBlockInt = Hex2Bin(Ipv6GetBlock(ip, blockNbr))
End Function

'----------------------------------------------
'   Ipv6SetBlock
'----------------------------------------------
' Sets the value of the 4-digit hexa block at position blockNbr
' The value of blockNbr can be 1 to 8, block 1 is the block on the left.
' example:
'   Ipv6SetBlock("2001::"; 2; "db8") returns "2001:0db8::"
Function Ipv6SetBlock(ByVal ip As String, blockNbr As Integer, ByVal valHex As String) As String
    ' make valHex exactly 4 characters long
    valHex = Right("0000" & valHex, 4)
    ip = Ipv6Expand(ip)
    Mid(ip, blockNbr * 5 - 4, 4) = valHex
    Ipv6SetBlock = Ipv6Compress(ip)
End Function

'----------------------------------------------
'   Ipv6SetBlockInt
'----------------------------------------------
' Same as above except that the block value is passed as an integer between
' 0 and 65535
Function Ipv6SetBlockInt(ByVal ip As String, blockNbr As Integer, valInt As Double) As String
    Dim valHex As String
    valHex = LCase(Hex(valInt And 65535))
    Ipv6SetBlockInt = Ipv6SetBlock(ip, blockNbr, valHex)
End Function

'----------------------------------------------
'   Ipv6SetBits
'----------------------------------------------
' Sets on or more bits in a ip v6 addresse
' bits is a string with on or more "0" and "1"
' offset is the position of the first bit to set between 1 to 128 from left to right
Function Ipv6SetBits(ByVal ip As String, bits As String, offset As Integer) As String
    Dim ipbin As String
    ipbin = Ipv6ToBin(ip) ' convert to binary
    Mid(ipbin, offset) = bits ' set the bits
    ipbin = Left(ipbin, 128) ' make sure we do not exceed 128 bits
    Ipv6SetBits = Ipv6FromBin(ipbin)
End Function

'----------------------------------------------
'   Ipv6GetIpv4
'----------------------------------------------
' Get the value of an IPv4 in an IPv6 at a given position
' exemple:
'    Ipv6GetIpv4("2001:c0a8:102::"; 2) returns "192.168.1.2"
Function Ipv6GetIpv4(ByVal ipv6 As String, blockNbr As Integer) As String
    Ipv6GetIpv4 = IpBinToStr(Ipv6GetBlockInt(ipv6, blockNbr) * 65536 + Ipv6GetBlockInt(ipv6, blockNbr + 1))
End Function

'----------------------------------------------
'   Ipv6SetIpv4
'----------------------------------------------
' Put the value of an IPv4 in an IPv6 at a given position
' exemple:
'    Ipv6SetIpv4("2001::"; 2; "192.168.1.2") returns "2001:c0a8:102::"
Function Ipv6SetIpv4(ByVal ipv6 As String, blockNbr As Integer, ByVal ipv4 As String) As String
    Dim result As String
    
    byte1 = IpParse(ipv4)
    byte2 = IpParse(ipv4)
    byte3 = IpParse(ipv4)
    byte4 = IpParse(ipv4)
    
    result = Ipv6SetBlockInt(ipv6, blockNbr + 1, byte1 + 256 * byte2)
    Ipv6SetIpv4 = Ipv6SetBlockInt(result, blockNbr, byte3 + 256 * byte4)
End Function

'----------------------------------------------
'   Ipv6SubnetFirstAddress
'----------------------------------------------
' example:
'   Ipv6SubnetFirstAddress("2001:db8:1:1a0::/59") returns "2001:db8:1:1a0::"
Function Ipv6SubnetFirstAddress(ByVal subnet As String) As Variant
    prefixlen = Ipv6MaskLen(subnet)
    Ipv6SubnetFirstAddress = Ipv6SetBits(subnet, String(128 - prefixlen, "0"), prefixlen + 1)
End Function

'----------------------------------------------
'   Ipv6SubnetLastAddress
'----------------------------------------------
' example:
'   Ipv6SubnetLastAddress("2001:db8:1:1a0::/59") returns "2001:db8:1:1bf:ffff:ffff:ffff:ffff"
Function Ipv6SubnetLastAddress(ByVal subnet As String) As Variant
    prefixlen = Ipv6MaskLen(subnet)
    Ipv6SubnetLastAddress = Ipv6SetBits(subnet, String(128 - prefixlen, "1"), prefixlen + 1)
End Function

Function Hex2Bin(ByVal strHex As String) As Double
    Dim v As Double
    For i = 1 To Len(strHex)
        v = 16 * v + Val("&H" & Mid$(strHex, i, 1))
    Next
    Hex2Bin = v
End Function

'----------------------------------------------
' OWN CODE
'----------------------------------------------

'----------------------------------------------
'   IpSetHostBits
'----------------------------------------------
' set to zero the bits in the host part of an address
' example:
'   IpSetHostBits("192.168.1.1/24") returns "192.168.1.255/24"
'   IpSetHostBits("192.168.1.193 255.255.255.128") returns "192.168.1.255 255.255.255.128"
Function IpSetHostBits(ByVal net As String) As String
	IpSetHostBits = IpOr(net, IpInvertMask(IpMask(net)))
End Function

'----------------------------------------------
'   IpNextSubnet
'----------------------------------------------
' returns the next subnet
' example:
'   IpSetHostBits("192.168.1.1/24") returns "192.168.1.255/24"
'   IpSetHostBits("192.168.1.193 255.255.255.128") returns "192.168.1.255 255.255.255.128"
Function IpNextSubnet(ByVal net As String) As String
    Dim ip As String
    ip = IpWithoutMask(net)
    IpNextSubnet = IpAdd(IpAnd(ip, IpMask(net)), IpSubnetSize(net)) + Mid(net, Len(ip) + 1)
End Function

'----------------------------------------------
'   IpGetSubnetNumber
'----------------------------------------------
' Gets the number of the subnet (reverse divide subnet)
' 'subnet' the smaller subnet
' 'n' difference between sub- and supernet, e.g. 8
' example:
'   IpGetSubnetNumber("1.2.3.0/26"; 2) returns "0"
'   IpGetSubnetNumber("1.2.3.64/26"; 2) returns "1"
Function IpGetSubnetNumber(ByVal subnet As String, n As Integer)
    Dim ip As String
    Dim supernet As String
    Dim slen As Integer
    Dim number As Integer
    ip = IpAnd(IpWithoutMask(subnet), IpMask(subnet))
    slen = IpSubnetLen(subnet)
    supernet = IpAnd(IpWithoutMask(subnet), IpMask("/" + Str(slen - n)))
    If (slen < 0) Then
        IpGetSubnetNumber = "ERR subnet length < 0"
        Exit Function
    End If
    IpGetSubnetNumber = (IpStrToBin(ip) - IpStrToBin(supernet)) / 2 ^ (32 - slen)
End Function
