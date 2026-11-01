"""Upload current catalog references; never overwrite differing immutable server input."""
import argparse
from build_workflow import read_json
from verify_visual_matrix import ensure_reference

if __name__ == '__main__':
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--server', default='http://127.0.0.1:8188')
    args = parser.parse_args()
    for product in read_json('presets.json')['products'].values():
        ensure_reference(args.server, product)
        print('Verified:', product['id'], product.get('poseGuide', '(text only)'))