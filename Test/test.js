import http from 'k6/http';
import { check, sleep } from 'k6';

export const options = {
    vus: 300,
    duration: '10m',
};

export function setup() {
    const loginRes = http.post(
        'https://localhost:7168/Auth/sign-in',
        JSON.stringify({
            usernameorEmail: 'Student@gmail.com',
            password: 'Student@123'
        }),
        {
            headers: {
                'Content-Type': 'application/json'
            }
        }
    );

    check(loginRes, {
        'login successful': (r) => r.status === 200,
    });

    const token = loginRes.json('accessToken');

    return { token };
}

export default function (data) {
    const res = http.get(
        'https://localhost:7168/Student/View-Enrolled-Subjects',
        {
            headers: {
                Authorization: `Bearer ${data.token}`
            }
        }
    );

    check(res, {
        'subjects loaded': (r) => r.status === 200,
    });

    sleep(1);
}

// score without redis
//  █ TOTAL RESULTS

//     checks_total.......: 31440  52.043236/s
//     checks_succeeded...: 99.82% 31386 out of 31440
//     checks_failed......: 0.17%  54 out of 31440

//     ✓ login successful
//     ✗ subjects loaded
//       ↳  99% — ✓ 31385 / ✗ 54

//     HTTP
//     http_req_duration..............: avg=4.73s min=0s       med=4.69s max=7.13s p(90)=5.12s p(95)=5.26s
//       { expected_response:true }...: avg=4.74s min=697.53ms med=4.69s max=7.13s p(90)=5.13s p(95)=5.26s
//     http_req_failed................: 0.17%  54 out of 31440
//     http_reqs......................: 31440  52.043236/s

//     EXECUTION
//     iteration_duration.............: avg=5.74s min=1s       med=5.69s max=9s    p(90)=6.13s p(95)=6.27s
//     iterations.....................: 31439  52.041581/s
//     vus............................: 17     min=17          max=300
//     vus_max........................: 300    min=300         max=300

//     NETWORK
//     data_received..................: 14 MB  23 kB/s
//     data_sent......................: 1.9 MB 3.1 kB/s




// running (10m04.1s), 000/300 VUs, 31439 complete and 0 interrupted iterations
// default ✓ [======================================] 300 VUs  10m0s


// // score with redis
//wsl
// TOTAL RESULTS

//     checks_total.......: 43549  72.085238/s
//     checks_succeeded...: 99.82% 43472 out of 43549
//     checks_failed......: 0.17%  77 out of 43549

//     ✓ login successful
//     ✗ subjects loaded
//       ↳  99% — ✓ 43471 / ✗ 77

//     HTTP
//     http_req_duration..............: avg=3.14s min=0s       med=3.1s max=4.71s p(90)=3.6s  p(95)=3.77s
//       { expected_response:true }...: avg=3.15s min=591.67ms med=3.1s max=4.71s p(90)=3.6s  p(95)=3.77s
//     http_req_failed................: 0.17%  77 out of 43549
//     http_reqs......................: 43549  72.085238/s

//     EXECUTION
//     iteration_duration.............: avg=4.14s min=1s       med=4.1s max=5.71s p(90)=4.61s p(95)=4.77s
//     iterations.....................: 43548  72.083583/s
//     vus............................: 33     min=33          max=300
//     vus_max........................: 300    min=300         max=300

//     NETWORK
//     data_received..................: 18 MB  29 kB/s
//     data_sent......................: 2.3 MB 3.9 kB/s




// running (10m04.1s), 000/300 VUs, 43548 complete and 0 interrupted iterations
// default ✓ [======================================] 300 VUs  10m0s

//docker
// █ TOTAL RESULTS

//     checks_total.......: 43172  71.486859/s
//     checks_succeeded...: 99.91% 43136 out of 43172
//     checks_failed......: 0.08%  36 out of 43172

//     ✓ login successful
//     ✗ subjects loaded
//       ↳  99% — ✓ 43135 / ✗ 36

//     HTTP
//     http_req_duration..............: avg=3.18s min=0s       med=3.11s max=6.57s p(90)=3.68s p(95)=3.94s
//       { expected_response:true }...: avg=3.18s min=691.74ms med=3.11s max=6.57s p(90)=3.68s p(95)=3.94s
//     http_req_failed................: 0.08%  36 out of 43172
//     http_reqs......................: 43172  71.486859/s

//     EXECUTION
//     iteration_duration.............: avg=4.18s min=1s       med=4.11s max=7.57s p(90)=4.69s p(95)=4.94s
//     iterations.....................: 43171  71.485204/s
//     vus............................: 164    min=164         max=300
//     vus_max........................: 300    min=300         max=300

//     NETWORK
//     data_received..................: 18 MB  29 kB/s
//     data_sent......................: 2.3 MB 3.9 kB/s




// running (10m03.9s), 000/300 VUs, 43171 complete and 0 interrupted iterations
// default ✓ [======================================] 300 VUs  10m0s