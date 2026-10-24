"""Metro Redux (2033 / Last Light) 用の簡易パッカー。

metro_redux_insert_newvfs.au3 (swuforce) の Python 版。バニラの content.vfx を元に、
指定したファイルだけを新しい content99.vfs0 に入れ、vfx をそれに向け直す。

au3 との違い:
  - フルパス (content\\...) で照合するので、別フォルダの同名ファイルを取り違えない
  - バニラに無いファイルも追加できる (既存フォルダの子の末尾に足す。MetroPacker と同じ)
  - 照合できないファイルがあればエラーにして何も書かない

使い方:
  python redux_pack.py <バニラ content.vfx> <入力フォルダ> <出力フォルダ> [--flat]

  入力フォルダ: 既定では content\\... の階層を持つフォルダ (中の content フォルダの親を指定)。
  --flat: 入力フォルダ直下のファイルをファイル名で照合する (au3 / exe と同じ置き方。
          make.bat の out フォルダ用)。バニラに同名ファイルが複数あるとエラー。
  出力フォルダに content.vfx と content99.vfs0 を書く。

形式 (調査結果。memo 02 / 11 を参照):
  vfx: ver u32, comp u32, guid 16B, npkg u32, nent u32, ndup u32,
       pkg 表 (name cstr, nlevels u32, level cstr*, vfs サイズ u32),
       エントリ表 (flags u16; フォルダ(flags&8): 子の数 u16, 最初の子 u32;
                   ファイル: pkg u16, off u32, 展開後サイズ u32, 圧縮後サイズ u32;
                   名前: len u8 (末尾 0 込み), xor u8, 文字列 xor 済み, 0),
       重複表 (20 byte × ndup。最後の u32 がエントリ番号), 末尾 8 byte
  vfs0: [u32 0][u32 データ長+24][データ][u32 3][u32 16][guid 16B]。オフセットは先頭から
"""
import argparse
import os
import struct
import sys

VFS_NAME = 'content99.vfs0'


class Entry:
    __slots__ = ('flags', 'a', 'b', 'c', 'd', 'name_raw', 'name')

    # フォルダ: a = 子の数, b = 最初の子。ファイル: a = pkg, b = off, c = 展開後, d = 圧縮後
    def is_dir(self):
        return bool(self.flags & 8)

    def pack(self):
        if self.is_dir():
            head = struct.pack('<HHI', self.flags, self.a, self.b)
        else:
            head = struct.pack('<HHIII', self.flags, self.a, self.b, self.c, self.d)
        return head + self.name_raw


def make_name(name, key):
    raw = name.encode('latin1')
    if len(raw) + 1 > 255:
        raise SystemExit('名前が長すぎる: ' + name)
    return bytes([len(raw) + 1, key]) + bytes(x ^ key for x in raw) + b'\0'


class Vfx:
    def __init__(self, path):
        b = open(path, 'rb').read()
        self.ver, self.comp = struct.unpack_from('<II', b, 0)
        if self.ver != 1:
            raise SystemExit('Redux の vfx ではない (version %d)' % self.ver)
        self.guid = b[8:24]
        npkg, nent, ndup = struct.unpack_from('<III', b, 24)
        o = 36
        self.pkgs = []
        for _ in range(npkg):
            s = o
            e = b.index(0, o)
            name = b[o:e].decode('latin1')
            o = e + 1
            n, = struct.unpack_from('<I', b, o)
            o += 4
            for _ in range(n):
                o = b.index(0, o) + 1
            o += 4
            self.pkgs.append((name, b[s:o]))
        self.ents = []
        for _ in range(nent):
            en = Entry()
            en.flags, = struct.unpack_from('<H', b, o)
            if en.flags & 8:
                en.a, en.b = struct.unpack_from('<HI', b, o + 2)
                en.c = en.d = None
                o += 8
            else:
                en.a, en.b, en.c, en.d = struct.unpack_from('<HIII', b, o + 2)
                o += 16
            ln, key = b[o], b[o + 1]
            en.name_raw = b[o:o + 2 + ln]
            en.name = bytes(x ^ key for x in b[o + 2:o + 1 + ln]).decode('latin1')
            o += 2 + ln
            self.ents.append(en)
        self.dups = [list(struct.unpack_from('<HHIIII', b, o + 20 * k)) for k in range(ndup)]
        o += 20 * ndup
        self.tail = b[o:]

    def paths(self):
        """小文字のフルパス → エントリ番号のリスト。フォルダは末尾に \\ を付けない"""
        out = {}
        stack = [(0, '')]
        while stack:
            i, pre = stack.pop()
            en = self.ents[i]
            p = pre + en.name
            if en.is_dir():
                out.setdefault(p.lower(), []).append(i)
                for j in range(en.b, en.b + en.a):
                    stack.append((j, p + '\\' if p else ''))
            else:
                out.setdefault(p.lower(), []).append(i)
        return out

    def insert_file(self, parent, name):
        """フォルダ parent の子の末尾にファイルを足し、その番号を返す"""
        pe = self.ents[parent]
        pos = pe.b + pe.a
        key = self.ents[pe.b].name_raw[1] if pe.a else 0
        en = Entry()
        en.flags, en.a, en.b, en.c, en.d = 0, 0, 0, 0, 0
        en.name_raw = make_name(name, key)
        en.name = name
        for k, x in enumerate(self.ents):
            if x.is_dir() and k != parent and x.b >= pos:
                x.b += 1
        pe.a += 1
        if pe.a > 0xFFFF:
            raise SystemExit('フォルダの子が多すぎる')
        for d in self.dups:
            if d[5] >= pos:
                d[5] += 1
        self.ents.insert(pos, en)
        return pos

    def write(self, path, new_pkg):
        out = [struct.pack('<II', self.ver, self.comp), self.guid,
               struct.pack('<III', len(self.pkgs) + 1, len(self.ents), len(self.dups))]
        out += [raw for _, raw in self.pkgs]
        out.append(new_pkg)
        out += [en.pack() for en in self.ents]
        out += [struct.pack('<HHIIII', *d) for d in self.dups]
        out.append(self.tail)
        with open(path, 'wb') as f:
            f.write(b''.join(out))


def collect(src, flat, vfx, pm):
    """(vfx 上のパス, 入力ファイルの絶対パス) のリスト"""
    items = []
    if flat:
        by_name = {}
        for key, idx in pm.items():
            for i in idx:
                if not vfx.ents[i].is_dir():
                    by_name.setdefault(vfx.ents[i].name.lower(), []).append(key)
        for fn in sorted(os.listdir(src)):
            full = os.path.join(src, fn)
            if not os.path.isfile(full):
                continue
            cand = by_name.get(fn.lower(), [])
            if len(cand) != 1:
                raise SystemExit('%s: バニラの vfx に %d 個ある (--flat では 1 個だけのファイルしか置けない)' % (fn, len(cand)))
            items.append((cand[0], full))
    else:
        for root, dirs, files in os.walk(src):
            dirs.sort()
            for fn in sorted(files):
                full = os.path.join(root, fn)
                rel = os.path.relpath(full, src).replace('/', '\\')
                items.append((rel, full))
    if not items:
        raise SystemExit('入力ファイルが無い: ' + src)
    return items


def main():
    ap = argparse.ArgumentParser(description=__doc__, formatter_class=argparse.RawDescriptionHelpFormatter)
    ap.add_argument('vfx')
    ap.add_argument('src')
    ap.add_argument('outdir')
    ap.add_argument('--flat', action='store_true')
    a = ap.parse_args()

    vfx = Vfx(a.vfx)
    if any(n.lower() == VFS_NAME for n, _ in vfx.pkgs):
        raise SystemExit('入力の vfx にすでに %s がある。バニラの vfx を指定すること' % VFS_NAME)
    pm = vfx.paths()
    items = collect(a.src, a.flat, vfx, pm)
    pkg_id = len(vfx.pkgs)

    # 既存エントリへの割り当てと新規追加を決める。追加でエントリ番号がずれるので、
    # 対象はエントリオブジェクトで持つ
    targets = []
    added = []
    for rel, full in items:
        key = rel.lower()
        if not key.startswith('content\\'):
            raise SystemExit('content\\ 以下に無い: ' + rel)
        idx = pm.get(key)
        if idx:
            if vfx.ents[idx[0]].is_dir():
                raise SystemExit('フォルダと同名: ' + rel)
            targets.append(([vfx.ents[i] for i in idx], full, rel))
            continue
        parent_key, _, name = key.rpartition('\\')
        pidx = pm.get(parent_key)
        if not pidx or not vfx.ents[pidx[0]].is_dir():
            raise SystemExit('親フォルダがバニラの vfx に無い (フォルダの新規作成は未対応): ' + rel)
        name = rel.rpartition('\\')[2]
        targets.append(None)
        added.append((pidx[0], name, full, rel, len(targets) - 1))
    # 追加するたびにエントリ番号がずれるので、親はエントリオブジェクトで持っておく
    parents = [(vfx.ents[p], name, full, rel, t) for p, name, full, rel, t in added]
    for pe, name, full, rel, t in parents:
        pos = vfx.insert_file(vfx.ents.index(pe), name)
        targets[t] = ([vfx.ents[pos]], full, rel)

    # 置き換えたエントリを指す重複表の行は消す (古い中身を指したままになるため)
    replaced = set()
    for ens, _, _ in targets:
        replaced.update(id(e) for e in ens)
    ent_ids = {id(e): k for k, e in enumerate(vfx.ents)}
    rep_idx = {ent_ids[x] for x in replaced}
    before = len(vfx.dups)
    vfx.dups = [d for d in vfx.dups if d[5] not in rep_idx]
    dropped = before - len(vfx.dups)

    os.makedirs(a.outdir, exist_ok=True)
    data = []
    pos = 8
    for ens, full, rel in targets:
        blob = open(full, 'rb').read()
        for en in ens:
            en.a, en.b, en.c, en.d = pkg_id, pos, len(blob), len(blob)
        data.append(blob)
        pos += len(blob)
    body = b''.join(data)
    vfs = struct.pack('<II', 0, len(body) + 24) + body + struct.pack('<II', 3, 16) + vfx.guid
    vfs_path = os.path.join(a.outdir, VFS_NAME)
    vfx_path = os.path.join(a.outdir, 'content.vfx')
    with open(vfs_path, 'wb') as f:
        f.write(vfs)
    new_pkg = VFS_NAME.encode('latin1') + b'\0' + struct.pack('<II', 0, len(vfs))
    vfx.write(vfx_path, new_pkg)

    added_rels = {x[3] for x in added}
    for ens, full, rel in targets:
        print('%s %s (%d)' % ('追加' if rel in added_rels else '置換', rel, os.path.getsize(full)))
    print('ファイル %d (うち追加 %d)、重複表から削除 %d 行' % (len(targets), len(added), dropped))
    print('出力:', vfx_path, vfs_path)


if __name__ == '__main__':
    main()
