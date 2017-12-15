# excel-ip-calc

Code from: [rohmer.fr](http://trk.free.fr/ipcalc/index.html)

**Additional functions:**

- `IpSetHostBits`: Sets all host bits to 1 (e.g. `IpSetHostBits("192.168.0.0/24")` returns `192.168.0.255`)
- `IpNextSubnet`: Returns the next same sized network (e.g. `IpNextSubnet("192.168.0.0/24")` returns `192.168.1.0/24`)
- `IpGetSubnetNumber`: Calculates `index` from `IpDivideSubnet` (e.g. `IpGetSubnetNumber("192.168.10.0/24",8)` returns `10`)
