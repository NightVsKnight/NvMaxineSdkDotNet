#
# From https://github.com/mmatrosov/DllDispatcher/blob/master/DllDispatcher.py
#

import sys
import struct

IMAGE_FILE_MACHINE_I386 = 0x014C
IMAGE_FILE_MACHINE_IA64 = 0x0200
IMAGE_FILE_MACHINE_AMD64 = 0x8664

def read_pe_machine_type(file_path):
    with open(file_path, "rb") as f:
        s = f.read(2)
        if s.decode() != 'MZ':
            raise OSError('File not recognized as PE.')

        f.seek(60)
        s = f.read(4)
        header_offset = struct.unpack("<L", s)[0]
        f.seek(header_offset + 4)
        s = f.read(2)
        return struct.unpack("<H", s)[0]

def main():
    dll_path = sys.argv[1]
    #print('dll_path', dll_path)
    machine = read_pe_machine_type(dll_path)
    if machine == IMAGE_FILE_MACHINE_I386:
        print('IMAGE_FILE_MACHINE_I386')
    elif machine == IMAGE_FILE_MACHINE_IA64:
        print('IMAGE_FILE_MACHINE_IA64')
    elif machine == IMAGE_FILE_MACHINE_AMD64:
        print('IMAGE_FILE_MACHINE_AMD64')
    else:
        print("UNKNOWN")

if __name__ == '__main__':
    main()
