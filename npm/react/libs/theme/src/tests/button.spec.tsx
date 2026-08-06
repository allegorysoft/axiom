import { describe, it, expect } from 'vitest';
import { render, screen } from '@testing-library/react';
import { Button } from '../components/ui/button';

describe('Button', () => {
  it('should render with default classes', () => {
    render(<Button>Click me</Button>);

    const button = screen.getByRole('button', { name: 'Click me' });

    expect(button).toHaveClass('bg-primary');
    expect(button).toHaveClass('h-8');
  });

  it('should apply variant and size', () => {
    render(
      <Button variant="destructive" size="lg">
        Delete
      </Button>,
    );

    const button = screen.getByRole('button', { name: 'Delete' });

    expect(button).toHaveClass('bg-destructive/10');
    expect(button).toHaveClass('h-9');
  });
});
