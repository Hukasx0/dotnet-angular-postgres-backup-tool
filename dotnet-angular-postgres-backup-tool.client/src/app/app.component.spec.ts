import { ComponentFixture, TestBed, fakeAsync, tick } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { AppComponent, BackupLogEntry } from './app.component';

/**
 * Test suite for the AppComponent
 * Tests component initialization, data loading, and utility functions
 */
describe('AppComponent', () => {
  let component: AppComponent;
  let fixture: ComponentFixture<AppComponent>;
  let httpMock: HttpTestingController;

  // Sample backup data for testing
  const mockLatestBackup: BackupLogEntry = {
    id: 1,
    databaseName: 'TestDB',
    backupDate: '2024-03-15T10:00:00',
    backupPath: '/backups/latest.sql',
    status: 1,
    backupSizeBytes: 1024000,
    duration: '00:05:30'
  };

  const mockBackups: BackupLogEntry[] = [
    mockLatestBackup,
    {
      id: 2,
      databaseName: 'TestDB',
      backupDate: '2024-03-14T10:00:00',
      backupPath: '/backups/old1.sql',
      status: 1,
      backupSizeBytes: 1024000,
      duration: '00:04:30'
    },
    {
      id: 3,
      databaseName: 'TestDB',
      backupDate: '2024-03-13T10:00:00',
      backupPath: '/backups/old2.sql',
      status: 2,
      errorMessage: 'Connection error'
    }
  ];

  /**
   * Setup before each test
   * Configures testing module and creates component instance
   */
  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      declarations: [AppComponent]
    }).compileComponents();

    fixture = TestBed.createComponent(AppComponent);
    component = fixture.componentInstance;
    httpMock = TestBed.inject(HttpTestingController);
  });

  /**
   * Cleanup after each test
   * Verifies that there are no outstanding HTTP requests
   */
  afterEach(() => {
    httpMock.verify();
  });

  /**
   * Test: Component Creation
   * Verifies that the component is created successfully
   */
  it('should create', () => {
    expect(component).toBeTruthy();
  });

  /**
   * Test: Initial Data Loading
   * Verifies that both latest backup and backup history are loaded on init
   */
  it('should load initial data on init', fakeAsync(() => {
    // Trigger ngOnInit
    fixture.detectChanges();

    // Handle latest backup request
    const latestReq = httpMock.expectOne('/api/Backup/latest');
    expect(latestReq.request.method).toBe('GET');
    latestReq.flush(mockLatestBackup);

    // Handle backups history request
    const backupsReq = httpMock.expectOne('/api/Backup');
    expect(backupsReq.request.method).toBe('GET');
    backupsReq.flush(mockBackups);

    // Let all promises resolve
    tick();

    // Verify component state
    expect(component.lastBackup).toEqual(mockLatestBackup);
    expect(component.backups).toEqual(mockBackups.slice(1));
  }));

  /**
   * Test: Duration Formatting
   * Verifies that the duration formatting function works correctly
   */
  describe('formatDuration', () => {
    it('should handle undefined duration', () => {
      expect(component.formatDuration(undefined)).toBe('N/A');
    });

    it('should format hours, minutes and seconds', () => {
      expect(component.formatDuration('01:30:45'))
        .toBe('1 hour, 30 minutes, 45 seconds');
    });

    it('should format minutes and seconds only', () => {
      expect(component.formatDuration('00:05:30'))
        .toBe('5 minutes, 30 seconds');
    });

    it('should format seconds only', () => {
      expect(component.formatDuration('00:00:30'))
        .toBe('30 seconds');
    });

    it('should handle single units correctly', () => {
      expect(component.formatDuration('01:01:01'))
        .toBe('1 hour, 1 minute, 1 second');
    });
  });

  /**
   * Test: Error Handling
   * Verifies that the component handles API errors gracefully
   */
  it('should handle API errors gracefully', fakeAsync(() => {
    // Create spy for console.error
    const consoleSpy = spyOn(console, 'error');
    
    // Trigger ngOnInit
    fixture.detectChanges();

    // Simulate error response for latest backup
    const latestReq = httpMock.expectOne('/api/Backup/latest');
    latestReq.error(new ErrorEvent('API error'));

    // Simulate error response for backup history
    const backupsReq = httpMock.expectOne('/api/Backup');
    backupsReq.error(new ErrorEvent('API error'));

    tick();

    // Verify error handling
    expect(component.lastBackup).toBeUndefined();
    expect(component.backups).toEqual([]);
    expect(consoleSpy).toHaveBeenCalled();
  }));

  /**
   * Test: Loading Latest Backup
   * Verifies that loadLastBackup updates component state correctly
   */
  it('should update lastBackup when loadLastBackup is called', fakeAsync(() => {
    component.loadLastBackup();

    const req = httpMock.expectOne('/api/Backup/latest');
    req.flush(mockLatestBackup);

    tick();

    expect(component.lastBackup).toEqual(mockLatestBackup);
  }));

  /**
   * Test: Loading Backup History
   * Verifies that loadBackups updates component state correctly
   */
  it('should update backups when loadBackups is called', fakeAsync(() => {
    component.loadBackups();

    const req = httpMock.expectOne('/api/Backup');
    req.flush(mockBackups);

    tick();

    expect(component.backups).toEqual(mockBackups.slice(1));
  }));
});