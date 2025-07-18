from multiprocessing import shared_memory
import ctypes

class SharedMemoryString:
    def __init__(self, name: str, capacity: int = 1024):
        self.name = name
        self.capacity = capacity
        try:
            #Try connecting to existing shared memory
            self.shm = shared_memory.SharedMemory(name=name)
            self.is_owner = False
        except FileNotFoundError:
            #Create new shared memory
            self.shm = shared_memory.SharedMemory(name=name, create=True, size=capacity)
            self.is_owner = True

    def write(self, message: str):
        encoded = message.encode('utf-8')
        length = len(encoded)
        if length + 1 > self.capacity:
            raise ValueError("Message too large for shared memory capacity")

        buf = self.shm.buf
        buf[0] = length
        buf[1:1+length] = encoded

    def read(self) -> str | None:
        buf = self.shm.buf
        length = buf[0]
        if length == 0 or length >= self.capacity:
            return None
        return bytes(buf[1:1+length]).decode('utf-8')

    def close(self):
        self.shm.close()
        if self.is_owner:
            self.shm.unlink()
