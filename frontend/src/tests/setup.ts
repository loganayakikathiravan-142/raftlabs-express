import '@testing-library/jest-dom';
import { beforeEach } from 'vitest';

// Clean localStorage before each test
beforeEach(() => {
  localStorage.clear();
});
