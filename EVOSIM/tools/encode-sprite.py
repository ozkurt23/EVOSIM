"""Lossless format conversion only: PNG RGBA -> SDL BGRA with width/height header."""
from pathlib import Path
from PIL import Image
import struct,sys
source,destination=map(Path,sys.argv[1:3])
im=Image.open(source)
if im.mode!='RGBA' or im.getchannel('A').getextrema()[0]!=0:raise ValueError('A real transparent RGBA source is required')
destination.write_bytes(struct.pack('<ii',*im.size)+im.tobytes('raw','BGRA'))
print(source.name,im.size,'alpha',im.getchannel('A').getextrema())
