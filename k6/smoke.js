import http from 'k6/http';
import { check, sleep } from 'k6';

export const options = {
  vus: 1,
  duration: '30s',
  thresholds: {
    http_req_failed: ['rate==0'],
    http_req_duration: ['p(99)<500'],
  },
};

const BASE_URL = __ENV.BASE_URL || 'https://localhost:7229';

export default function () {
  const res = http.get(`${BASE_URL}/api/catalogue?page=1&pageSize=20`);
  check(res, {
    'status is 200': (r) => r.status === 200,
    'has items': (r) => r.json('items').length > 0,
  });
  sleep(1);
}