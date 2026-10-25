import http from 'k6/http';
import { check } from 'k6';

const BASE_URL = __ENV.BASE_URL || 'https://localhost:7229';
const VUS = Number(__ENV.VUS || 100);

export const options = {
  scenarios: {
    steady_load: {
      executor: 'constant-vus',
      vus: VUS,
      duration: '1m',
    },
  },
  thresholds: {
    http_req_failed: ['rate<0.01'],
    http_req_duration: ['p(95)<100', 'p(99)<250'],
  },
};

const GENRES = ['Sci-Fi', 'Action', 'Drama', 'Comedy', 'Horror', 'Thriller', 'Romance', 'Documentary'];

export default function () {
  const page = Math.floor(Math.random() * 50) + 1;
  const url = Math.random() < 0.7
    ? `${BASE_URL}/api/catalogue?page=${page}&pageSize=20`
    : `${BASE_URL}/api/catalogue?genre=${GENRES[Math.floor(Math.random() * GENRES.length)]}&page=${page}&pageSize=20`;

  const res = http.get(url);
  check(res, { 'status is 200': (r) => r.status === 200 });
}