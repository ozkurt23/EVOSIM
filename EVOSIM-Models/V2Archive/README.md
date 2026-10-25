# Forest v2 timeline boundary

The public ForestSession facade wraps the proven timeline/store helpers compiled against the separate CoreV2 assembly. Original v1helpers/binary/user archives are unchanged. V2default store identity is a compositeSHA256of actual CoreV2, ForestEcology and V2Archive assemblies with named ordered entries. Changes in ecology or command/replay code therefore invalidate old histories even if the CoreV2file alone is unchanged. Never replace this with the old single-Core hash.

Create(World), Advance, SetDrought, Seek/Pump, ReturnToHead, Branch, Save(directory,name,existingId), Open(directory,id), ExportTimeline and ValidateTimeline form the host boundary. Save always saves the authoritative head even during historical preview. Branch establishes an independent origin. The standard serialized archive format retains its strict model/checkpoint validation.

V1 archives must be read under LegacyBridge and explicitly imported as a new v2origin. This facade does not accept them implicitly. No user archive was converted during validation.

v2archive-verification44.json records12passing cases: changing forest, exact pre-death historical forest, preserved head, independent branch, preview-safe save, reopened origin, exact complete replay, continued save, foreign model rejection, and sensitivity to each of the3model assemblies.

This is a library/API, not an integrated Godot menu or finished distribution. Godot model routing, old-world import UX, seven ecological settings and live tree visual updates remain pending. Large-map throughput and coupled long-run balance are not established.
